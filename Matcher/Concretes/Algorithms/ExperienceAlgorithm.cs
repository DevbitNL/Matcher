﻿//           ■■■■   ■■■■■■              
//           ■■■■   ■■■■■■        © Copyright 2014         
//           ■■■■         ■■■■     _____             _     _ _           _
//           ■■■■   ■■■   ■■■■    |  __ \           | |   (_) |         | |
//           ■■■■   ■■■   ■■■■    | |  | | _____   _| |__  _| |_   _ __ | |
//           ■■■■         ■■■■    | |  | |/ _ \ \ / / '_ \| | __| | '_ \| |     
//           ■■■■■■■■■■■■■        | |__| |  __/\ V /| |_) | | |_ _| | | | |
//           ■■■■■■■■■■■■■        |_____/ \___| \_/ |_.__/|_|\__(_)_| |_|_|


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communicator;
using Matcher.Concretes.Algorithms;

namespace Matcher.Algorithms
{

    class ExperienceAlgorithm : IAlgorithm
    {
        
        // Matched Experience and Details
        public MatchFactor CalculateFactor<T>(Profile profile, Vacancy vacancy, int multiplier)
        {
            TextAnalyser analyser = new TextAnalyser(10, "http://api.stackoverflow.com/1.1/tags?pagesize=100&page=");
            MatchFactorFactory matchFactorFactory = new CustomMatchFactorFactory();

            List<Experience> experiences = profile.experience;
            
            int count = 0; 
            double score = 0;

            for (int expNr = 0; expNr < experiences.Count; expNr++)
            {
                Experience experience = experiences.ElementAt(expNr);

                List<string> matchingWordsExperience = analyser.AnalyseText(experience.description);
                if (matchingWordsExperience != null)
                {
                    matchingWordsExperience.AddRange(analyser.AnalyseText(experience.details)); // Also check experience Details
                    matchingWordsExperience.AddRange(analyser.AnalyseText(experience.title)); // Also check experience Title
                    matchingWordsExperience.Add(profile.interests);
                    matchingWordsExperience.Add(profile.specialties);
                }

                if (profile.honors != null)
                {
                    for (int i = 0; i < profile.honors.Count; i++)
                    {
                        string honour = profile.honors.ElementAt(i);
                        matchingWordsExperience.Add(honour);
                    }  
                }        

                List<string> matchingWordsVacancy = analyser.AnalyseText(vacancy.details.advert_html);
                List<string> matchingWordsCombined = analyser.CompareLists(
                    matchingWordsExperience,
                    matchingWordsVacancy);

                double preScore = 0;
                if (matchingWordsCombined.Count != 0)
                {
                    preScore += (matchingWordsCombined.Count / Math.Min(matchingWordsVacancy.Count, matchingWordsExperience.Count)) * 100;
                }

                // Systeem voor gewerkte tijd per experience.
                string start = experience.start;
                string end = experience.end;
                int startInteger = 0;
                int endInteger = 0;

                if (end != null && (end.ToLower().Equals("present") || end.ToLower().Equals("heden")))
                {
                    endInteger = Convert.ToInt32(DateTime.UtcNow.Date.ToString("yyyyMM"));
                }
                else if (end != null)
                {
                    endInteger = TranslateDate(end);
                }

                if (start != null)
                {
                    startInteger = TranslateDate(start);
                }
                int timeWorked = 0;
                if (startInteger != 0 || endInteger != 0)
                {
                    timeWorked = FixDateInt(endInteger - startInteger);
                }

                if (timeWorked > 500)
                {
                    preScore *= 1;
                }
                else
                {
                    double threshold = 0.3;
                    preScore *= 1.0 - threshold + (timeWorked / 500 * threshold);
                }

                score += preScore;
                count++;
            }

            // Gemiddelde voor eerlijke score
            if (count != 0 && score != 0)
            {
                MatchFactor experienceFactor = matchFactorFactory.CreateMatchFactor("Experience", "", Convert.ToDouble(count) / score, multiplier);
                return experienceFactor;
            }
            return null;
        }

        private int TranslateDate(string date)
        {
            date = date.ToLower();

            int year = 0;
            int month = 0;

            if (date.Contains("januari") || date.Contains("january"))
            {
                month = 1;
            }
            else if (date.Contains("februari") || date.Contains("february"))
            {
                month = 2;
            }
            else if (date.Contains("maart") || date.Contains("march"))
            {
                month = 3;
            }
            else if (date.Contains("april"))
            {
                month = 4;
            }
            else if (date.Contains("mei") || date.Contains("may"))
            {
                month = 5;
            }
            else if (date.Contains("juni") || date.Contains("june"))
            {
                month = 6;
            }
            else if (date.Contains("juli") || date.Contains("july"))
            {
                month = 7;
            }
            else if (date.Contains("augustus") || date.Contains("august"))
            {
                month = 8;
            }
            else if (date.Contains("september"))
            {
                month = 9;
            }
            else if (date.Contains("oktober"))
            {
                month = 10;
            }
            else if (date.Contains("november"))
            {
                month = 11;
            }
            else if (date.Contains("december"))
            {
                month = 12;
            }
            if (month > 0)
            {
                year = Convert.ToInt32(date.Substring(0, Math.Max(0, date.Length - 4))) * 100;
            }
            else
            {
                year = Convert.ToInt32(date)*100;
            }

            return (month + year);
        }

        private int FixDateInt(int dateInt)
        {
            string dateIntString = Convert.ToString(dateInt);
            int partial = Math.Max(0, dateIntString.Length - 2);
            string actualData = dateIntString.Substring(partial);
            int monthFixed = Convert.ToInt32(actualData) % 12;

            if (monthFixed == 0)
            {
                monthFixed = 1;
            }
            int year = Convert.ToInt32(dateIntString.Substring(0, partial)) * 100;
            int result = year + monthFixed;
            return result;
        }
               
    }
}
