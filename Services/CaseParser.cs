using System.Text.RegularExpressions;
using Proffessional.Models;

namespace Proffessional.Services
{
    public static class CaseParser
    {
        public static List<TowingCase> Parse(string message)
        {
            var clean = Normalize(message);
            var blocks = SplitIntoCases(clean);

            var list = new List<TowingCase>();

            foreach (var b in blocks)
            {
                var tc = new TowingCase
                {
                    CaseId = Get(b,
                        @"Case\s*(ID|Number)\s*[:\-]?\s*([A-Z0-9\-]+)",
                        @"\b(AT\d+|SRN\d+|RZ-\d+|OLAE-\d+)\b"
                    ),

                    CustomerName = Get(b,
                        @"Customer\s*Name\s*[:\-]?\s*(.*?)(?=\s*(Vehicle|Model|Registration|Chassis|Contact|Incident|$))",
                        @"Name\s*[:\-]?\s*(.*?)(?=\s*(Vehicle|Model|Registration|$))"
                    ),

                    VehicleBrand = Get(b,
                        @"Vehicle\s*Make\s*/?\s*Brand\s*[:\-]?\s*(.*?)(?=\s*Model|Registration|$)",
                        @"MAKE/MODEL/VARIANT\s*[:\-]?\s*(.*?)(?=$)"
                    ),

                    Model = Get(b,
                        @"Model\s*[:\-]?\s*(.*?)(?=\s*Registration|Chassis|$)"
                    ),

                    RegistrationNo = Get(b,
                        @"Registration\s*(No|Number)\s*[:\-]?\s*([A-Z0-9]+)",
                        @"\bKL\d{2}[A-Z0-9]{4,}\b"
                    ),

                    ChassisNo = Get(b,
                        @"Chassis\s*No\s*[:\-]?\s*([A-Z0-9]+)",
                        @"VIN\s*NUMBER\s*[:\-]?\s*([A-Z0-9]+)"
                    ),

                    CustomerContactNumber = Get(b,
                        @"Customer\s*(Contact|Phone)\s*Number\s*[:\-]?\s*(\d{10})",
                        @"\+91[- ]?(\d{10})"
                    ),

                    IncidentReason = Get(b,
                        @"Incident\s*Reason\s*[:\-]?\s*(.*?)(?=\s*Incident\s*Place|Drop|$)",
                        @"Issue\s*[:\-]?\s*(.*?)(?=\s*Pickup|Drop|$)"
                    ),

                    IncidentPlace = Get(b,
                        @"Incident\s*Place\s*[:\-]?\s*(.*?)(?=\s*Drop|$)",
                        @"Pickup\s*(Location)?\s*[:\-]?\s*(.*?)(?=\s*Drop|$)"
                    ),

                    DropLocation = Get(b,
                        @"Drop\s*(Location)?\s*[:\-]?\s*(.*?)(?=\s*Assigned|Vendor|$)"
                    ),

                    AssignedVendorName = Get(b,
                        @"Assigned\s*Vendor\s*Name\s*[:\-]?\s*(.*?)(?=\s*Vendor\s*Contact|$)"
                    ),

                    VendorContactNumber = Get(b,
                        @"Vendor\s*Contact\s*Number\s*[:\-]?\s*(\d{10})"
                    ),

                    TowingType = Get(b,
                        @"Towing\s*Type\s*[:\-]?\s*(.*?)(?=\s*TOLL|$)",
                        @"Service\s*Type\s*[-:]?\s*(\S+)"
                    )
                };

                if (!string.IsNullOrWhiteSpace(tc.CaseId))
                    list.Add(tc);
            }

            return list;
        }

        // ================= HELPERS =================

        private static string Normalize(string text)
        {
            text = Regex.Replace(text, @"https?:\/\/\S+", "");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        private static List<string> SplitIntoCases(string text)
        {
            return Regex.Split(
                text,
                @"(?=\b(Case\s*(ID|Number)|SRN\d+|AT\d+|OLAE-\d+|RZ-\d+)\b)",
                RegexOptions.IgnoreCase
            )
            .Select(x => x.Trim())
            .Where(x => x.Length > 40)
            .ToList();
        }

        private static string Get(string text, params string[] patterns)
        {
            foreach (var p in patterns)
            {
                var m = Regex.Match(text, p,
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (m.Success)
                    return m.Groups[m.Groups.Count - 1].Value.Trim();
            }
            return "";
        }
    }
}
