using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Brief
{
    public enum enmMailingType
    {
        Relatie,
        Adviseur
    }
    public class XMLMailingConverter
    {
        const string conSeperator= ";";
        public void ConvertToCsv(string parXmlString,enmMailingType parType,string parMap,string parNaam)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(parXmlString);

            string sFileName = parNaam + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            using (var File = new StreamWriter(Path.Combine(parMap, sFileName)))
            {
                var aData = new ArrayList();
                var bHeadersAlGeschreven = false;

                foreach (XmlNode BriefNode in xmlDoc.DocumentElement.SelectNodes("brief"))
                {
                    aData.Clear();
                    switch (parType)
                    {
                        case enmMailingType.Adviseur:
                            VulRelatie(BriefNode.SelectSingleNode("adviseur"), "", aData, true);
                            VulAdviseur(BriefNode.SelectSingleNode("adviseur"), aData);
                            VulContactpersoon(BriefNode.SelectSingleNode("contactpersoon"), aData);
                            break;
                        case enmMailingType.Relatie:
                            VulVerzekerde(BriefNode.SelectSingleNode("verzekerde"), aData);
                            VulRelatie(BriefNode.SelectSingleNode("verzekeringnemer"), "verzekeringnemer", aData, true);
                            VulRelatie(BriefNode.SelectSingleNode("adviseur"), "adviseur", aData, false);
                            VulSchades(BriefNode.SelectSingleNode("schades"), aData);
                            VulPolissen(BriefNode.SelectSingleNode("polissen"), aData);
                            break;
                    }

                    if (!bHeadersAlGeschreven)
                    {
                        SchrijfVeld(File, aData,true);
                        bHeadersAlGeschreven = true;
                    }

                    SchrijfVeld(File, aData,false);
                }
            }
        }

        private void SchrijfVeld(StreamWriter parFile, ArrayList parVelden,bool parHeader)
        {
            bool varEerste = true;
            foreach (Veld v in parVelden)
            {
                if (varEerste)
                    varEerste = false;
                else
                    parFile.Write(conSeperator);

                if (parHeader)
                    parFile.Write(v.Kop);
                else
                    parFile.Write(v.Waarde);
            }

            parFile.WriteLine();
        }

        private void VulContactpersoon(XmlNode parContactpersoonNode,ArrayList parData)
        {
            parData.Add(new Veld("achternaam contactpersoon", parContactpersoonNode.SelectSingleNode("achternaam").InnerText));
            parData.Add(new Veld("voorletters contactpersoon", parContactpersoonNode.SelectSingleNode("voorletters").InnerText));
            parData.Add(new Veld("voorletters salesmanager", parContactpersoonNode.SelectSingleNode("salesmanager").SelectSingleNode("voorletters").InnerText));
            parData.Add(new Veld("naam salesmanager", parContactpersoonNode.SelectSingleNode("salesmanager").SelectSingleNode("naam").InnerText));
        }
        private void VulAdviseur(XmlNode parAdviseurNode,ArrayList parData)
        {
            parData.Add(new Veld("kopiegeprint", parAdviseurNode.SelectSingleNode("kopiegeprint").InnerText));
            parData.Add(new Veld("isincasserend", parAdviseurNode.SelectSingleNode("isincasserend").InnerText));
        }
        private void VulPolissen(XmlNode parPolissenNode,ArrayList parData)
        {
            XmlNodeList Polissen = parPolissenNode.SelectNodes("polis");
            for (int iCounter=1;iCounter<=8;iCounter++)
            {
                parData.Add(new Veld("polisnummer" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("polisnummer").InnerText));
                parData.Add(new Veld("indexcode" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("indexcode").InnerText));
                parData.Add(new Veld("contractduur" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("contractduur").InnerText));
                parData.Add(new Veld("eindleeftijd" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("eindleeftijd").InnerText));
                parData.Add(new Veld("object" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("object").InnerText));
                parData.Add(new Veld("dekking" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("dekking").InnerText));
                parData.Add(new Veld("contracteinddatum" + iCounter, Polissen.Count < iCounter ? "" : Polissen[iCounter - 1].SelectSingleNode("contracteinddatum").InnerText));
            }
        }
        private void VulSchades(XmlNode parSchadesNode,ArrayList parData)
        {
            var sHeeftSchade = "";
            var sHeeftCNMN = "";

            if (parSchadesNode!=null)
            {
                sHeeftSchade = "J";
                foreach ( XmlNode Node in parSchadesNode.SelectNodes("schade"))
                {
                    if (Node.InnerText == "J")
                        sHeeftCNMN = "J";
                }
            }

            parData.Add(new Veld("HeeftSchade", sHeeftSchade));
            parData.Add(new Veld("HeeftCNMN", sHeeftCNMN));
        }
        private void VulVerzekerde(XmlNode parVerzekerdeNode,ArrayList parData)
        {
            parData.Add(new Veld("relatienummer verzekerde", parVerzekerdeNode.SelectSingleNode("relatienummer").InnerText));
            parData.Add(new Veld("geboortedatum verzekerde", parVerzekerdeNode.SelectSingleNode("geboortedatum").InnerText));
            parData.Add(new Veld("geslacht verzekerde", parVerzekerdeNode.SelectSingleNode("geslacht").InnerText));
            parData.Add(new Veld("voorletters verzekerde", parVerzekerdeNode.SelectSingleNode("voorletters").InnerText));
            parData.Add(new Veld("naam verzekerde", parVerzekerdeNode.SelectSingleNode("naam").InnerText));
            parData.Add(new Veld("beroep verzekerde",parVerzekerdeNode.SelectSingleNode("beroep").InnerText));
            parData.Add(new Veld("heeftBeroepsorganisatie verzekerde",parVerzekerdeNode.SelectSingleNode("heeftBeroepsorganisatie").InnerText));

            VulNAW(parVerzekerdeNode.SelectSingleNode("naw"), "verzekerde", parData);
        }
        private void VulRelatie(XmlNode parRelatieNode,string parSuffixHeader,ArrayList parData,bool parNAW)
        {
            parData.Add(new Veld(("relatienummer " + parSuffixHeader).Trim(), parRelatieNode == null ? "" : parRelatieNode.SelectSingleNode("relatienummer").InnerText));
            parData.Add(new Veld(("naam " + parSuffixHeader).Trim(), parRelatieNode == null ? "" : parRelatieNode.SelectSingleNode("naam").InnerText));

            if (parNAW)
                VulNAW(parRelatieNode == null ? null : parRelatieNode.SelectSingleNode("naw"), parSuffixHeader, parData);
        }
        private void VulNAW(XmlNode parNAWNode,string parSuffixHeader, ArrayList parData)
        {
            parData.Add(new Veld(("straat " + parSuffixHeader).Trim(), parNAWNode==null ? "" : parNAWNode.SelectSingleNode("straat").InnerText));
            parData.Add(new Veld(("huisnummer " + parSuffixHeader).Trim(), parNAWNode == null ? "" : parNAWNode.SelectSingleNode("huisnummer").InnerText));
            parData.Add(new Veld(("toevoeging " + parSuffixHeader).Trim(), parNAWNode == null ? "" : parNAWNode.SelectSingleNode("toevoeging").InnerText));
            parData.Add(new Veld(("plaats " + parSuffixHeader).Trim(), parNAWNode == null ? "" : parNAWNode.SelectSingleNode("plaats").InnerText));
            parData.Add(new Veld(("landcode " + parSuffixHeader).Trim(), parNAWNode == null ? "" : parNAWNode.SelectSingleNode("landcode").InnerText));
            parData.Add(new Veld(("land " + parSuffixHeader).Trim(), parNAWNode == null ? "" : parNAWNode.SelectSingleNode("land").InnerText));
            parData.Add(new Veld(("postcode " + parSuffixHeader).Trim(), parNAWNode == null ? "" : parNAWNode.SelectSingleNode("postcode").InnerText));
        }

        private class Veld
        {
            public string Kop { get; private set; }
            public string Waarde { get; private set; }

            public Veld(string parKop,string parWaarde)
            {
                Kop = parKop;
                Waarde = parWaarde;
            }
        }
    }
}
