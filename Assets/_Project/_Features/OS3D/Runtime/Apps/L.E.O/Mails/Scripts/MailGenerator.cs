using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using LightHouse.Features.TimeOfDay.TimeCore;

namespace LightHouse.Features.Computer.LEO.Mails
{ 
    public static class MailGenerator
    {
        #region ======================== Data types ========================
        /// <summary>
        /// Style générique pour le rendu TMP (couleurs, tailles, interlignage, séparateur).
        /// </summary>
        [Serializable]
        public struct MailStyle
        {
            public string Primary;   // titres
            public string Accent;    // sous-titres / infos
            public string Positive;  // ok/gains
            public string Negative;  // erreurs/alertes
            public string Body;      // texte principal

            public int TitlePct;     // % taille titre
            public int BodyPct;      // % taille corps
            public float LineHeight;   // % line-height

            // Optionnel: séparateur visuel
            public string Divider;     // "—" ou "• • •", etc.

            public static MailStyle Default => new MailStyle
            {
                Primary = "#1E88E5",
                Accent = "#8EACBB",
                Positive = "#00C853",
                Negative = "#FF5252",
                Body = "#E0E0E0",
                TitlePct = 150,
                BodyPct = 100,
                LineHeight = 110f,
                Divider = "────────────────"
            };
        }

        /// <summary>
        /// Ligne de bulletin météo affichable (déjà “cuisinée” côté gameplay).
        /// </summary>
        [Serializable]
        public class ForecastLine
        {
            public string Period;         // ex: "Morning", "Coast E/SE"…
            public int LowC;
            public int HighC;
            public int WindKts;
            public string SeaState;       // "Calm", "Moderate", "Rough"…
                                          // TO DOO changer pour échelle beaufort
            public string WindDir;        // "N", "NE", … (facultatif)
            public float ConfidencePct;  // 0..100 (facultatif)
            public string Note;           // commentaire libre

            public ForecastLine(
                string period, int lowC, int highC, int windKts, string seaState,
                string note = "", string windDir = "", float confidencePct = -1f)
            {
                Period = period;
                LowC = lowC;
                HighC = highC;
                WindKts = windKts;
                SeaState = seaState;
                WindDir = windDir;
                ConfidencePct = confidencePct;
                Note = note;
            }
        }
        #endregion

        #region ======================== TMP helpers (SRP: rendu/formatage) ========================
        /// <summary>
        /// Utilitaires d’assemblage de texte TMP (Single Responsibility: rendu).
        /// </summary>
        private static class Rt
        {
            public static string Escape(string s) => s?
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");

            public static string Money(float amount, string currencySymbol = "$")
                => string.Format(CultureInfo.InvariantCulture, "{0}{1:N0}", currencySymbol, amount);

            public static string Header(string subjectLeft, string fromRight, MailStyle st)
            {
                return
    $@"<line-height={st.LineHeight}%><size={st.BodyPct}%><color={st.Body}>
<align=left><size={st.TitlePct}%><b><color={st.Primary}>{Escape(subjectLeft)}</color></b></size></align>
<align=right><i><color={st.Accent}>{Escape(fromRight)}</color></i></align>
<align=left>";
            }

            public static string Footer(string signature)
                => $@"<br><br>Respectfully,<br>{Escape(signature)}</color>";

            public static string Divider(MailStyle st)
                => $@"<br><alpha=#55>{st.Divider}</alpha><br>";

            /// <summary>
            /// Colore un pourcentage via un code couleur simple.
            /// 0–59% = Negative, 60–79% = Accent, 80–100% = Positive
            /// </summary>
            public static string PercentColored(float pct, MailStyle st)
            {
                string col = pct >= 80f ? st.Positive : (pct >= 60f ? st.Accent : st.Negative);
                return $"<b><color={col}>{pct:0}%</color></b>";
            }
        }
        #endregion

        #region ======================== Forecast rendering (SRP: bulletin) ========================
        /// <summary>
        /// Rendu des lignes de prévision (séparé pour alléger les générateurs).
        /// </summary>
        private static class ForecastRender
        {
            /// <summary>
            /// Insère une ligne de bulletin (avec couleurs wind/sea + direction/Note/Confidence si présents).
            /// </summary>
            public static void WriteForecastLine(StringBuilder sb, MailStyle st, string title, ForecastLine f)
            {
                sb.AppendLine($"<br><b><color={st.Accent}>{Rt.Escape(title)}</color></b><br>");

                if (f == null)
                {
                    sb.AppendLine("<i>Forecast unavailable.</i><br>");
                    return;
                }

                string windCol = f.WindKts >= 25 ? st.Negative : (f.WindKts >= 15 ? st.Accent : st.Positive);
                string seaCol = (f.SeaState?.ToLowerInvariant().Contains("rough") ?? false) ? st.Negative : st.Body;

                sb.AppendLine($"• {Rt.Escape(f.Period)} — {f.LowC}–{f.HighC}°C<br>");

                string dirSuffix = string.IsNullOrEmpty(f.WindDir) ? "" : $" ({Rt.Escape(f.WindDir)})";
                sb.AppendLine($"• Wind: <color={windCol}>{f.WindKts} kts</color>{dirSuffix}<br>");
                sb.AppendLine($"• Sea state: <color={seaCol}>{Rt.Escape(f.SeaState)}</color><br>");

                if (f.ConfidencePct >= 0f)
                    sb.AppendLine($"• Confidence: {f.ConfidencePct:0}%<br>");

                if (!string.IsNullOrWhiteSpace(f.Note))
                    sb.AppendLine($"• Note: {Rt.Escape(f.Note)}<br>");
            }
        }
        #endregion

        #region ======================== Factory MailDatas ========================
        private static MailDatas MakeMailDatas(
            string expeditorName,
            string subject,
            byte arrivalDay,
            float arrivalTime,
            string richTextBody,
            MailAttachedFile[] files = null)
        {
            return new MailDatas
            {
                ExpeditorName = expeditorName,
                MailObject = subject,
                ArrivalDay = arrivalDay,
                ArrivalTime = arrivalTime,   // ex: 13.5f = 13h30
                MailMessage = richTextBody,
                Files = files
            };
        }
        #endregion

        #region ======================== NIGHTWATCH ========================
        /// <summary>
        /// Rapport Nightwatch : résumé boats/buoys + gains + note capitaine.
        /// </summary>
        public static MailDatas GenerateMailFromNightwatchTemplate(
            string dateFormat,
            string keeperName,
            int boatsCorrect,
            int boatsErrors,
            int buoysNominal,
            int buoysDefective,
            int buoysErrors,
            float totalEarnings,
            string captainsNote,
            // Métadonnées MailDatas :
            byte arrivalDay,
            float arrivalTime,
            // Options :
            string currencySymbol = "$",
            string expeditorLabel = "Harbor Master’s Office",
            MailStyle? style = null,
            MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;
            string subj = $"Nightwatch Report – {dateFormat}";
            string head = Rt.Header(subj, $"From : {expeditorLabel}", st);

            string boatsErrColor = boatsErrors > 0 ? st.Negative : st.Accent;
            string buoysErrColor = buoysErrors > 0 ? st.Negative : st.Accent;

            var sb = new StringBuilder();
            sb.AppendLine($"<br><b>Dear {Rt.Escape(keeperName)},</b><br><br>");
            sb.AppendLine("Please find below the summary of your night watch:<br><br>");

            sb.AppendLine("<b>Boats:</b><br>");
            sb.AppendLine($"• Correct reports: <b><color={st.Positive}>{boatsCorrect}</color></b><br>");
            sb.AppendLine($"• Errors: <b><color={boatsErrColor}>{boatsErrors}</color></b><br><br>");

            sb.AppendLine("<b>Buoys:</b><br>");
            sb.AppendLine($"• Correct nominal buoys: <b><color={st.Positive}>{buoysNominal}</color></b><br>");
            sb.AppendLine($"• Correct defective buoys: <b><color={st.Positive}>{buoysDefective}</color></b><br>");
            sb.AppendLine($"• Errors: <b><color={buoysErrColor}>{buoysErrors}</color></b><br><br>");

            sb.AppendLine($"<b>Total earnings: <color={st.Positive}>{Rt.Money(totalEarnings, currencySymbol)}</color></b><br><br>");
            sb.AppendLine("<b>Captain’s Note:</b><br>");
            sb.AppendLine($"<i>{Rt.Escape(captainsNote)}</i>");

            string foot = Rt.Footer(expeditorLabel);
            string message = head + sb.ToString() + foot;
            return MakeMailDatas(expeditorLabel, subj, arrivalDay, arrivalTime, message, files);
        }

        /// <summary> Exemple minimal Nightwatch (démo) </summary>
        public static MailDatas GenerateMailFromNightwatchTemplate()
        {
            return GenerateMailFromNightwatchTemplate(
                dateFormat: TimeUtility.FormatCurrentDate(),
                keeperName: "A. Morgan",
                boatsCorrect: 12,
                boatsErrors: 1,
                buoysNominal: 8,
                buoysDefective: 3,
                buoysErrors: 0,
                totalEarnings: 1750f,
                captainsNote: "Bravo, Keeper! Excellent work tonight: your vigilance keeps the waters safe.",
                arrivalDay: 3,
                arrivalTime: 9.0f
            );
        }
        #endregion

        #region ======================== WEATHER ========================
        /// <summary>
        /// Mail "Weather Report" : résultats d’exactitude, gains, note station, et bulletin (3 lignes).
        /// </summary>
        public static MailDatas GenerateMailFromWeatherTemplate(
            string dateFormat,
            string keeperName,
            // Exactitudes par métrique (0..100)
            float airTempAcc,
            float waterTempAcc,
            float humidityAcc,
            float windSpeedAcc,
            float windDirectionAcc,   // 0 ou 100 dans ton modèle
            float pressureAcc,
            // Gains
            float totalEarnings,
            // Prévisions (attend min 1 pour Today, 2 pour Tomorrow, 3 pour J+2)
            IList<ForecastLine> forecast,
            // Textes de note
            string stationNoteGood = "Excellent precision today. Your measurements are reliable and greatly help our forecasts.",
            string stationNoteBad = "Your report contained several inaccuracies. Please take more care with tomorrow’s readings: forecasts depend on reliable data.",
            // Métadonnées MailDatas
            byte arrivalDay = 0,
            float arrivalTime = 9.0f,
            // Options
            float accuracyThreshold = 75f,
            string currencySymbol = "$",
            string expeditorLabel = "Coastal Weather Station",
            MailStyle? style = null,
            MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;
            string subj = $"Weather Report – {dateFormat}";
            string head = Rt.Header(subj, $"From : {expeditorLabel}", st);

            // — Moyenne d'exactitude (6 métriques) —
            float avg = (airTempAcc + waterTempAcc + humidityAcc + windSpeedAcc + windDirectionAcc + pressureAcc) / 6f;
            bool forecastAvailable = avg >= accuracyThreshold;

            // — Note station —
            bool good = forecastAvailable;
            string noteTitleColor = good ? st.Positive : st.Negative;
            string noteText = good ? stationNoteGood : stationNoteBad;

            var sb = new StringBuilder();

            // — Intro —
            sb.AppendLine($"<br><b>Dear {Rt.Escape(keeperName)},</b><br><br>");
            sb.AppendLine("Please find below the summary & analysis of your daily weather report:<br>");
            sb.AppendLine(Rt.Divider(st));

            // — Bloc "Accuracy Results" —
            sb.AppendLine($"<size=115%><b><color={st.Primary}>Accuracy Results</color></b></size><br>");
            sb.AppendLine($"Air temperature: {Rt.PercentColored(airTempAcc, st)}<br>");
            sb.AppendLine($"Water temperature: {Rt.PercentColored(waterTempAcc, st)}<br>");
            sb.AppendLine($"Humidity rate: {Rt.PercentColored(humidityAcc, st)}<br>");
            sb.AppendLine($"Wind speed: {Rt.PercentColored(windSpeedAcc, st)}<br>");
            sb.AppendLine($"Wind direction: {Rt.PercentColored(windDirectionAcc, st)}<br>");
            sb.AppendLine($"Air pressure: {Rt.PercentColored(pressureAcc, st)}<br>");
            sb.AppendLine($"<i><color={st.Accent}>Average accuracy: {Rt.PercentColored(avg, st)}</color></i><br>");
            sb.AppendLine(Rt.Divider(st));

            // — Gains —
            sb.AppendLine($"<b>Total earnings: <color={st.Positive}>{Rt.Money(totalEarnings, currencySymbol)}</color></b><br>");
            sb.AppendLine(Rt.Divider(st));

            // — Note station —
            sb.AppendLine($"<size=115%><b><color={noteTitleColor}>Coastal Station’s Note</color></b></size><br>");
            sb.AppendLine($"<i>{Rt.Escape(noteText)}</i><br>");
            sb.AppendLine(Rt.Divider(st));

            // — Bulletin —
            sb.AppendLine("🌦 <size=115%><b><color={st.Primary}>Forecast Bulletin</color></b></size><br>");

            // Safe access 0/1/2
            ForecastLine f0 = (forecast != null && forecast.Count > 0) ? forecast[0] : null; // Today
            ForecastLine f1 = (forecast != null && forecast.Count > 1) ? forecast[1] : null; // Tomorrow
            ForecastLine f2 = (forecast != null && forecast.Count > 2) ? forecast[2] : null; // Day after tomorrow

            // Today: toujours affiché (fallback si null)
            ForecastRender.WriteForecastLine(
                sb, st, "Today", f0 ?? new ForecastLine("Today", 0, 0, 0, "—", "Based on today's report.")
            );

            // Tomorrow & Day+2 : conditionnés par le seuil d’exactitude moyen
            if (forecastAvailable)
            {
                ForecastRender.WriteForecastLine(sb, st, "Tomorrow", f1);
                ForecastRender.WriteForecastLine(sb, st, "Day after tomorrow", f2);
            }
            else
            {
                sb.AppendLine("<br><b><color=#FFA726>Tomorrow</color></b><br>");
                sb.AppendLine("<i>Forecast unavailable due to insufficient accuracy.</i><br>");
                sb.AppendLine("<br><b><color=#FFA726>Day after tomorrow</color></b><br>");
                sb.AppendLine("<i>Forecast unavailable due to insufficient accuracy.</i><br>");
            }

            // — Footer —
            string foot = Rt.Footer(expeditorLabel);
            string message = head + sb.ToString() + foot;
            return MakeMailDatas(expeditorLabel, subj, arrivalDay, arrivalTime, message, files);
        }
        #endregion


        // ====================== SUPPLIES ======================
        #region Types

        [Serializable]
        public sealed class SupplyOrderDatas
        {
            public int ProductId;
            public string Name;
            public int Quantity;
            public string Unit;        // "pcs", "kg", "L", etc. (optionnel)
            public float UnitPrice;   // Prix unitaire (dans la même monnaie que currencySymbol)
            public GameObject Prefab;

            public SupplyOrderDatas(int productId, string name, int quantity, float unitPrice, GameObject prefab, string unit = "")
            {
                ProductId = productId;
                Name = name;
                Quantity = quantity;
                Unit = unit;
                UnitPrice = unitPrice;
                Prefab = prefab;
            }

            public float LineTotal => Mathf.Max(0, Quantity) * UnitPrice;
        }

        #endregion

        #region Helpers (Supplies)

        /// <summary> Formate 0..24h en "hh:mm a.m./p.m." (ex: 9 -> "09:00 a.m.") </summary>
        private static string FormatTime12h(float hour24)
        {
            // Normalisation simple
            float clamped = Mathf.Repeat(hour24, 24f);
            int h = Mathf.FloorToInt(clamped);
            int m = Mathf.RoundToInt((clamped - h) * 60f);
            if (m >= 60) { m -= 60; h = (h + 1) % 24; }

            bool isPM = (h >= 12);
            int h12 = h % 12; if (h12 == 0) h12 = 12;

            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00} {2}",
                h12, m, isPM ? "p.m." : "a.m.");
        }

        #endregion

        #region Mail: Supply Order

        /// <summary>
        /// Génère un mail de commande de fournitures (Supply Order).
        /// Mise en forme TMP, avec:
        /// - liste des articles "Nom – Quantité – PrixLigne"
        /// - total calculé
        /// - livraison: "in X days at hh:mm a.m./p.m."
        /// - note station (OK / Delay) personnalisable
        /// </summary>
        public static MailDatas GenerateMailFromSupplyOrderTemplate(
            string dateFormat,
            string keeperName,
            IEnumerable<SupplyOrderDatas> items,
            int deliveryDay,
            float deliveryHour,                     // ex: 9f => 09:00 a.m.
                                                    // Métadonnées MailDatas:
            byte arrivalDay,
            float arrivalTime,
            uint ticketNumber,
            // Options:
            string expeditorLabel = "Coastal Trading Post",
            string currencySymbol = "$",
            string stationNoteOk = "No issues expected — shipment will arrive on time.",
            string stationNoteDelay = "Due to bad weather, your shipment may be delayed. We will keep you updated.",
            bool isDelayed = false,
            MailStyle? style = null,
            MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;

            // ---------- En-tête ----------
            string subject = $"Supply Order Confirmed";
            string head = Rt.Header(subject, $"From : {expeditorLabel}", st);

            // ---------- Corps ----------
            var sb = new StringBuilder();
            sb.AppendLine($"<br><b>Dear {Rt.Escape(keeperName)},</b><br><br>");
            sb.AppendLine($"Please find below the summary of your order [Ticket#{ticketNumber.ToString("000")}]:<br>");
            sb.AppendLine(Rt.Divider(st));

            // Titre "Ordered Items"
            sb.AppendLine($"<size=115%><b><color={st.Primary}>Ordered Items</color></b></size><br>");

            float total = 0f;
            int lineCount = 0;

            if (items != null)
            {
                foreach (var it in items)
                {
                    lineCount++;

                    // Quantité + unité
                    string qtyStr = it.Quantity.ToString(CultureInfo.InvariantCulture);
                    if (!string.IsNullOrWhiteSpace(it.Unit))
                        qtyStr = $"{qtyStr} {Rt.Escape(it.Unit)}";

                    float lineTotal = it.LineTotal;
                    total += lineTotal;

                    // • Name – Qty – Price
                    sb.Append("• ");
                    sb.Append($"<b>{Rt.Escape(it.Name)}</b> – {Rt.Escape(qtyStr)} – {Rt.Money(lineTotal, currencySymbol)}<br>");
                }
            }

            if (lineCount == 0)
                sb.AppendLine("<i>No items ordered.</i><br>");

            sb.AppendLine(Rt.Divider(st));

            // Total
            sb.AppendLine($"<b>Total Cost: <color={st.Positive}>{Rt.Money(total, currencySymbol)}</color></b><br>");

            // Livraison
            //Mails on envoie la date et pas le nombre d'heures
            string when = $"at {TimeUtility.FormatDate(deliveryDay, deliveryHour)}";
            sb.AppendLine($"Delivery: Shipment scheduled to be sent {when}.<br>");
            sb.AppendLine(Rt.Divider(st));

            // Note station
            string noteTitleColor = isDelayed ? st.Negative : st.Positive;
            string noteText = isDelayed ? stationNoteDelay : stationNoteOk;
            sb.AppendLine($"<size=115%><b><color={noteTitleColor}>Coastal Station’s Note</color></b></size><br>");
            sb.AppendLine($"<i>{Rt.Escape(noteText)}</i><br>");

            // ---------- Pied ----------
            string foot = Rt.Footer(expeditorLabel);
            string message = head + sb.ToString() + foot;

            return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, message, files);
        }

        #endregion

        public static MailDatas BuildShipmentDelayNotice(
        string dateFormat,
        string keeperName,
        uint ticketNumber,
        byte newDeliveryDay,
        float newDeliveryHour,          // ex: 9f
        byte arrivalDay,                // quand le mail arrive en boîte (meta)
        float arrivalTime,              // idem
        string expeditorLabel = "Coastal Trading Post",
        MailStyle? style = null,
        MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;

            // En-tête
            string subject = $"Supply Shipment Delayed";
            string head = Rt.Header(subject, $"From : {expeditorLabel}", st);

            // "Day XX - hh:mm AM/PM" (utilise ton utilitaire de formatage in-game)
            string prettyNewDate = TimeUtility.FormatDate(newDeliveryDay, newDeliveryHour);

            // Corps
            var sb = new StringBuilder();
            sb.AppendLine($"<br><b>Dear {Rt.Escape(keeperName)},</b><br><br>");
            sb.AppendLine($"We regret to inform you that your supply shipment [Ticket#{ticketNumber:000}] has been delayed due to bad weather conditions.<br><br>");
            sb.AppendLine($"The new estimated delivery date is <b>{Rt.Escape(prettyNewDate)}</b>.<br>");
            sb.AppendLine("<i>We will keep you updated if further changes occur.</i><br><br>");
            sb.AppendLine("Thank you for your understanding.");

            // Pied + MailDatas
            string foot = Rt.Footer(expeditorLabel);
            return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, head + sb.ToString() + foot, files);
        }

        public static MailDatas BuildSupplyDeliverySent(
        string dateFormat,
        string keeperName,
        uint ticketNumber,            // ex: 123 → s’affiche [#123]
        float etaHour = 9f,           // heure IN-GAME estimée d’arrivée (affichée en 12h “09:00 AM”)
                                      // Métadonnées d’arrivée du mail (quand il apparaît dans la boîte) :
        byte arrivalDay = 0,
        float arrivalTime = 9.0f,
        // Options UI :
        string expeditorLabel = "Coastal Trading Post",
        MailStyle? style = null,
        MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;

            // ---------- Header ----------
            string subject = $"Supply Delivery Sent";
            string head = Rt.Header(subject, $"From : {expeditorLabel}", st);

            // Heure estimée “autour de 9 a.m.”
            string etaPretty = TimeUtility.FormatTime12h(etaHour); // ex: "09:00 AM"

            // ---------- Body ----------
            var sb = new StringBuilder();
            sb.AppendLine($"<br><b>Dear {Rt.Escape(keeperName)},</b><br><br>");
            sb.AppendLine($"Your order <b>[Ticket#{ticketNumber:000}]</b> is ready to be dispatched.<br>");
            sb.AppendLine($"It should arrive around <b>{etaPretty.ToLowerInvariant()}</b>.<br><br>");
            sb.AppendLine("We will keep you informed of the delivery's arrival, so please stay tuned.<br><br>");
            sb.AppendLine("Best regards,");

            // ---------- Footer + MailDatas ----------
            string foot = Rt.Footer(expeditorLabel);
            string message = head + sb.ToString() + foot;

            return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, message, files);
        }

        public static MailDatas BuildSupplyDeliveryCompleted(
        string dateFormat,
        string keeperName,
        uint ticketNumber,            // ex: 123 → s’affiche [#123]
                                      // Métadonnées d’arrivée du mail (quand il apparaît dans la boîte) :
        byte arrivalDay = 0,
        float arrivalTime = 9.0f,
        // Options UI :
        string expeditorLabel = "Coastal Trading Post",
        MailStyle? style = null,
        MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;

            // ---------- Header ----------
            string subject = $"Supply Delivery Completed";
            string head = Rt.Header(subject, $"From : {expeditorLabel}", st);

            // ---------- Body ----------
            var sb = new StringBuilder();
            sb.AppendLine($"<br><b>Dear {Rt.Escape(keeperName)},</b><br><br>");
            sb.AppendLine($"The delivery for your order <b>[Ticket#{ticketNumber:000}]</b> has been successfully completed.<br>");
            sb.AppendLine("You will find your items in the designated storage area.<br><br>");
            sb.AppendLine("Best regards,");

            // ---------- Footer + MailDatas ----------
            string foot = Rt.Footer(expeditorLabel);
            string message = head + sb.ToString() + foot;

            return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, message, files);
        }

    }
}