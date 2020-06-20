using System;
using System.Globalization;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using SwissAcademic.Citavi;
using SwissAcademic.Citavi.Metadata;
using SwissAcademic.Citavi.Shell;
using SwissAcademic.Collections;


namespace ReferencesToolbox
{
    class ParallelReporterParser
    {
        public static void ParseReporters(Project project, List<Reference> listSelectedReferences)
        {
            string stringDateIn = string.Empty;
            DateTime dateTimeDate;
            string stringDateTimeYear = string.Empty;
            string stringDateTimeMonth = string.Empty;
            string stringDateTimeDay = string.Empty;
            string stringDateOut = string.Empty;
            CultureInfo provider = CultureInfo.InvariantCulture;

            List<string> neutralReportersCanadaList = new List<string>
            {
                "SCC", "CSC", "FC", "CF", "FCA", "CAF", "TCC", "CTC", "CMAC", "CACM", "Comp. Trib.", "Trib. Comp.", "CHRT", "TCDP", "PSSRB",
                "ABCA", "ABQB", "ABPC", "ABASC",
                "BCCA", "BCSC", "BCPC", "BCHRT", "BCSECCOM",
                "MBCA", "MBQB", "MBPC",
                "NBCA", "NBQB", "NBPC",
                "NFCA", "NLSCTD",
                "NSCA", "NSSC", "NSSF", "NSPC", "NSUARB", "NSBS",
                "NWTCA", "NWTSC", "NWTTC",
                "NUCJ", "NUCA",
                "ONCA", "ONSC", "ONCJ", "ONSWIAT", "ONLSAP", "ONLSHP",
                "PESCAD", "PESCTD",
                "QCCA", "QCSC", "QCCP", "QCTP", "CMQC", "QCCRT",
                "SKCA", "SKQB", "SKPC", "SKAIA",
                "YKCA", "YKSC", "YKTC", "YKSM", "YKYC",
				
				// Not actual Canadian Neutral Reporters, but reporters that follow the same format
				"S.L.T.", "S.C. (H.L.)"
            };

            foreach (Reference reference in listSelectedReferences)
            {
                if (reference.ReferenceType == ReferenceType.CourtDecision)
                {

                    stringDateIn = reference.Date;

                    string[] formats = new string[]
                    {
                        "dd MMMM yyyy",
                        "d MMMM yyyy",
                        "d MMM yyyy",
                        "d MMM yy",
                        "yyyy MMMM dd",
                        "yyyy MMMM d",
                        "dd/MM/yyyy",
                        "d/MM/yyyy",
                        "d/M/yyyy"
                    };

                    if (DateTime.TryParseExact(stringDateIn,
                                               formats,
                                               CultureInfo.InvariantCulture, //TODO: may be you want CultureInfo.CurrentCulture
                                               DateTimeStyles.AssumeLocal,
                                               out dateTimeDate))
                    {
                        stringDateTimeYear = dateTimeDate.Year.ToString();
                        if (dateTimeDate.Month > 9)
                        {
                            stringDateTimeMonth = dateTimeDate.Month.ToString();
                        }
                        else
                        {
                            stringDateTimeMonth = "0" + dateTimeDate.Month.ToString();
                        }
                        if (dateTimeDate.Day > 9)
                        {
                            stringDateTimeDay = dateTimeDate.Day.ToString();
                        }
                        else
                        {
                            stringDateTimeDay = "0" + dateTimeDate.Day.ToString();
                        }

                        stringDateOut = stringDateTimeYear + "-" + stringDateTimeMonth + "-" + stringDateTimeDay;
                        reference.Date = stringDateOut;
                    }
                    else
                    {
                        // log error: parsing fails
                    }

                    string jurisdiction = string.Empty;

                    Periodical newPeriodical = new Periodical(project, "");

                    IList<Person> organizationsList = reference.Organizations as IList<Person>;

                    Person court = organizationsList.FirstOrDefault();

                    if (court != null)
                    {
                        jurisdiction = court.LastNameForSorting;
                    }

                    string thePreferredReporterString = reference.CustomField1;
                    string theAdditionalReporterString = reference.CustomField2;

                    reference.Title = reference.Title.Replace(" v ", " v. ");
                    reference.Title = reference.Title.Replace("Inc", "Inc.");
                    reference.Title = reference.Title.Replace("Ltd", "Ltd.");
                    reference.Title = reference.Title.Replace("Plc", "Plc.");
                    reference.Title = reference.Title.Replace("..", ".");

                    List<string> thePreferredReporterList = reference.CustomField1.Split(new string[] { " and ", "; " }, StringSplitOptions.None).ToList();
                    List<string> theAdditionalReporterList = reference.CustomField1.Split(new string[] { " and ", "; " }, StringSplitOptions.None).ToList();
                    List<string> theParallelReporterList = reference.TitleSupplement.Split(new string[] { " and ", "; " }, StringSplitOptions.None).ToList();

                    List<string> allReportersList = thePreferredReporterList.Concat(theAdditionalReporterList).Concat(theParallelReporterList).ToList();

                    int i = 0;

                    foreach (string theReporterString in allReportersList)
                    {
                        if (string.IsNullOrEmpty(theReporterString)) continue;

                        string thePagesString = String.Empty;
                        string thePeriodicalString = String.Empty;
                        string theNumberString = String.Empty;
                        string theYearString = String.Empty;
                        string theVolumeString = String.Empty;
                        string theYearOrVolumeString = String.Empty;

                        List<string> theReporterList = theReporterString.Split(new string[] { " " }, StringSplitOptions.None).ToList();

                        List<string> thePeriodicalList = new List<string>();
                        List<string> theYearVolumeNumberAndPagesList = new List<string>();

                        foreach (string substring in theReporterList)
                        {
                            if (IsPeriodical(substring))
                            {
                                thePeriodicalList.Add(substring);
                            }
                            else
                            {
                                theYearVolumeNumberAndPagesList.Add(substring);
                            }
                        }

                        thePeriodicalString = String.Join(" ", thePeriodicalList);
                        bool neutralReporterCanadaBool = neutralReportersCanadaList.Any(thePeriodicalString.Contains);

                        if (neutralReporterCanadaBool)
                        {
                            theYearString = theYearVolumeNumberAndPagesList.FirstOrDefault();
                        }
                        else
                        {
                            theYearOrVolumeString = theYearVolumeNumberAndPagesList.FirstOrDefault();
                            if (IsDateYear(theYearOrVolumeString))
                            {
                                theYearOrVolumeString = theYearVolumeNumberAndPagesList[1];
                            }
                            if (IsYear(theYearOrVolumeString))
                            {
                                Regex regex = new Regex("\\[([0-9]*?)\\]");
                                var v = regex.Match(theYearOrVolumeString);
                                theYearString = v.Groups[1].ToString();
                            }
                            else
                            {
                                theVolumeString = theYearOrVolumeString;
                            }
                        }

                        if (theYearVolumeNumberAndPagesList.Count > 2 && !IsDateYear(theYearOrVolumeString))
                        {
                            theNumberString = theYearVolumeNumberAndPagesList[1];
                        }

                        thePagesString = theYearVolumeNumberAndPagesList.LastOrDefault();

                        List<string> messageList = new List<string> {"i: [" , i.ToString(), "]\n",
                                                                     "Reporters: ",  String.Join(", ", allReportersList), "\n",
                                                                     "Periodical: ", thePeriodicalString, "\n",
                                                                     "Periodical Year: " , theYearString, "\n",
                                                                     "Periodical Volume: ", theVolumeString, "\n",
                                                                     "Periodical Number: ", theNumberString, "\n",
                                                                     "Pages: ", thePagesString};

                        Periodical thePeriodical = new Periodical(project, thePeriodicalString);

                        foreach (Periodical periodical in project.Periodicals)
                        {
                            if (thePeriodicalString == periodical.StandardAbbreviation)
                            {
                                thePeriodical = periodical;
                                break;
                            }
                        }

                        if (i == 0)
                        {
                            MessageBox.Show(String.Join("", messageList));
                            if (reference.Periodical == null)
                            {
                               ModifyCurrentReference(reference, thePeriodical, theYearString, theVolumeString, theNumberString, thePagesString);
                            }
                            else
                            {
                               CreateNewReference(project, organizationsList, reference, thePeriodical, theYearString, theVolumeString, theNumberString, thePagesString);
                            }
                        }
                        else
                        {
                            MessageBox.Show(String.Join("", messageList));
                            CreateNewReference(project, organizationsList, reference, thePeriodical, theYearString, theVolumeString, theNumberString, thePagesString);
                        }

                        i = i + 1;
                    } // end foreach (string theReporterString in allReportersList)
                    reference.TitleSupplement = "";
                }
            }
        }
        private static bool IsPeriodical(String str)
        {
            return Regex.IsMatch(str, @"[\s]*[a-zA-Z&]+[\s]*"); // The expression matches any string that contains at least one letter
        }
        private static bool IsYear(String str)
        {
            return Regex.IsMatch(str, @"[\[][0-9]+[\]]"); //
        }
        private static bool IsDateYear(String str)
        {
            return Regex.IsMatch(str, @"[\(][0-9]+[\)]"); //
        }
        public static void ModifyCurrentReference(Reference reference, Periodical thePeriodical, string theYearString, string theVolumeString, string theNumberString, string thePagesString)
        {
            reference.Periodical = thePeriodical;
            reference.Year = theYearString;
            reference.Volume = theVolumeString;
            reference.Number = theNumberString;
            reference.PageRange = thePagesString;
            if (string.IsNullOrEmpty(reference.Date)) reference.Date = theYearString;
            reference.CustomField1 = "";
            reference.CustomField2 = "";
        }
        public static void CreateNewReference(Project activeProject, IList<Person> organizationsList, Reference reference, Periodical thePeriodical, string theYearString, string theVolumeString, string theNumberString, string thePagesString)
        {
            Reference newReference = new Reference(activeProject, ReferenceType.CourtDecision, "");
            newReference.Organizations.Add(organizationsList[0]);
            newReference.Title = reference.Title;
            newReference.CitationKey = reference.CitationKey;
            newReference.Periodical = thePeriodical;
            newReference.Date = reference.Date;
            newReference.Year = theYearString;
            newReference.Volume = theVolumeString;
            newReference.SpecificField2 = reference.SpecificField2;
            newReference.Number = theNumberString;
            newReference.PageRange = thePagesString;
            newReference.CustomField4 = reference.CustomField4;
            activeProject.References.Add(newReference);
        }

    }
}
