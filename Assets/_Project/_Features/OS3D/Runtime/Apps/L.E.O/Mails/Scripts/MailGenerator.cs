using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using LightHouse.Features.TimeOfDay.TimeCore;

namespace LightHouse.Features.Computer.LEO.Mails
{
    public static class MailGenerator
    {
        #region ======================== Data types ========================
        /// <summary>
        /// Ligne de bulletin météo affichable (déjà "cuisinée" côté gameplay).
        /// </summary>
        [Serializable]
        public class ForecastLine
        {
            public string Period;         // ex: "Morning", "Coast E/SE"…
            public int LowC;
            public int HighC;
            public int WindKts;
            public string SeaState;       // "Calm", "Moderate", "Rough"…
                                          // TODO changer pour échelle beaufort
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

        [Serializable]
        public sealed class SupplyOrderDatas
        {
            public int ProductId;
            public string Name;
            public int Quantity;
            public string Unit;        // "pcs", "kg", "L", etc. (optionnel)
            public float UnitPrice;    // Prix unitaire (dans la même monnaie que currencySymbol)
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

        #region ======================== Legacy factory (compat) ========================
        /// <summary>
        /// Conservé pour compatibilité avec du code existant hors de ce fichier.
        /// Pour tout nouveau template, préférer MailBuilder.From(...).Build().
        /// </summary>
        [Obsolete("Utiliser MailBuilder.From(expeditorLabel, style).Subject(...).Body(...).Build() à la place.", false)]
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
                ArrivalTime = arrivalTime,
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
            string boatsErrColor = boatsErrors > 0 ? st.Negative : st.Accent;
            string buoysErrColor = buoysErrors > 0 ? st.Negative : st.Accent;

            return MailBuilder.From(expeditorLabel, st)
                .Subject($"Nightwatch Report – {dateFormat}")
                .Arrival(arrivalDay, arrivalTime)
                .Attach(files)
                .Body(t => t
                    .NewLine().Bold($"Dear {keeperName},").NewLine(2)
                    .Line("Please find below the summary of your night watch:").NewLine()
                    .Bold("Boats:").Line()
                    .Bullet(b => b.Raw("Correct reports: ").Positive(boatsCorrect.ToString(), bold: true))
                    .Bullet(b => b.Raw("Errors: ").Color(boatsErrors.ToString(), boatsErrColor, bold: true))
                    .NewLine()
                    .Bold("Buoys:").Line()
                    .Bullet(b => b.Raw("Correct nominal buoys: ").Positive(buoysNominal.ToString(), bold: true))
                    .Bullet(b => b.Raw("Correct defective buoys: ").Positive(buoysDefective.ToString(), bold: true))
                    .Bullet(b => b.Raw("Errors: ").Color(buoysErrors.ToString(), buoysErrColor, bold: true))
                    .NewLine()
                    .Bold("Total earnings: ").Positive(MailFormat.Money(totalEarnings, currencySymbol), bold: true).NewLine(2)
                    .Bold("Captain’s Note:").Line()
                    .Italic(captainsNote))
                .Build();
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
        /// Mail "Weather Report" : résultats d'exactitude, gains, note station, et bulletin (3 lignes).
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

            float avg = (airTempAcc + waterTempAcc + humidityAcc + windSpeedAcc + windDirectionAcc + pressureAcc) / 6f;
            bool forecastAvailable = avg >= accuracyThreshold;
            string noteTitleColor = forecastAvailable ? st.Positive : st.Negative;
            string noteText = forecastAvailable ? stationNoteGood : stationNoteBad;

            ForecastLine f0 = (forecast != null && forecast.Count > 0) ? forecast[0] : null; // Today
            ForecastLine f1 = (forecast != null && forecast.Count > 1) ? forecast[1] : null; // Tomorrow
            ForecastLine f2 = (forecast != null && forecast.Count > 2) ? forecast[2] : null; // Day after tomorrow

            return MailBuilder.From(expeditorLabel, st)
                .Subject($"Weather Report – {dateFormat}")
                .Arrival(arrivalDay, arrivalTime)
                .Attach(files)
                .Body(t => t
                    .NewLine().Bold($"Dear {keeperName},").NewLine(2)
                    .Line("Please find below the summary & analysis of your daily weather report:")
                    .Divider()

                    .SectionTitle("Accuracy Results")
                    .Raw("Air temperature: ").Percent(airTempAcc).Raw("<br>")
                    .Raw("Water temperature: ").Percent(waterTempAcc).Raw("<br>")
                    .Raw("Humidity rate: ").Percent(humidityAcc).Raw("<br>")
                    .Raw("Wind speed: ").Percent(windSpeedAcc).Raw("<br>")
                    .Raw("Wind direction: ").Percent(windDirectionAcc).Raw("<br>")
                    .Raw("Air pressure: ").Percent(pressureAcc).Raw("<br>")
                    .Raw("<i>").Accent("Average accuracy: ").Percent(avg).Raw("</i><br>")
                    .Divider()

                    .Bold("Total earnings: ").Positive(MailFormat.Money(totalEarnings, currencySymbol), bold: true).Raw("<br>")
                    .Divider()

                    .SectionTitle("Coastal Station’s Note", noteTitleColor)
                    .Italic(noteText).Raw("<br>")
                    .Divider()

                    .Raw("🌦 ").SectionTitle("Forecast Bulletin")
                    .ForecastBlock("Today", f0 ?? new ForecastLine("Today", 0, 0, 0, "—", "Based on today's report."))
                    .If(forecastAvailable,
                        then: b => b
                            .ForecastBlock("Tomorrow", f1)
                            .ForecastBlock("Day after tomorrow", f2),
                        otherwise: b => b
                            .NewLine().Color("Tomorrow", "#FFA726", bold: true).NewLine()
                            .Italic("Forecast unavailable due to insufficient accuracy.").Raw("<br>")
                            .NewLine().Color("Day after tomorrow", "#FFA726", bold: true).NewLine()
                            .Italic("Forecast unavailable due to insufficient accuracy.").Raw("<br>")))
                .Build();
        }
        #endregion

        #region ======================== SUPPLIES ========================

        /// <summary>
        /// Génère un mail de commande de fournitures (Supply Order).
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
            string noteTitleColor = isDelayed ? st.Negative : st.Positive;
            string noteText = isDelayed ? stationNoteDelay : stationNoteOk;
            string when = $"at {TimeUtility.FormatDate(deliveryDay, deliveryHour)}";

            float total = 0f; // accumulé pendant le .Each ci-dessous, utilisé juste après

            return MailBuilder.From(expeditorLabel, st)
                .Subject("Supply Order Confirmed")
                .Arrival(arrivalDay, arrivalTime)
                .Attach(files)
                .Body(t => t
                    .NewLine().Bold($"Dear {keeperName},").NewLine(2)
                    .Line($"Please find below the summary of your order [Ticket#{ticketNumber:000}]:")
                    .Divider()
                    .SectionTitle("Ordered Items")
                    .Each(items, (b, it) =>
                    {
                        string qtyStr = string.IsNullOrWhiteSpace(it.Unit)
                            ? it.Quantity.ToString(CultureInfo.InvariantCulture)
                            : $"{it.Quantity.ToString(CultureInfo.InvariantCulture)} {it.Unit}";
                        total += it.LineTotal;

                        b.Bullet(x => x
                            .Bold(it.Name)
                            .Raw(" – ").Text(qtyStr)
                            .Raw(" – ").Text(MailFormat.Money(it.LineTotal, currencySymbol)));
                    },
                        emptyText: "No items ordered.")
                    .Divider()
                    .Bold("Total Cost: ").Positive(MailFormat.Money(total, currencySymbol), bold: true).Raw("<br>")
                    .Line($"Delivery: Shipment scheduled to be sent {when}.")
                    .Divider()
                    .SectionTitle("Coastal Station’s Note", noteTitleColor)
                    .Italic(noteText).Raw("<br>"))
                .Build();
        }

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
            string prettyNewDate = TimeUtility.FormatDate(newDeliveryDay, newDeliveryHour);

            return MailBuilder.From(expeditorLabel, st)
                .Subject("Supply Shipment Delayed")
                .Arrival(arrivalDay, arrivalTime)
                .Attach(files)
                .Body(t => t
                    .NewLine().Bold($"Dear {keeperName},").NewLine(2)
                    .Line($"We regret to inform you that your supply shipment [Ticket#{ticketNumber:000}] has been delayed due to bad weather conditions.")
                    .NewLine()
                    .Raw("The new estimated delivery date is ").Bold(prettyNewDate).Raw(".<br>")
                    .Italic("We will keep you updated if further changes occur.").Raw("<br><br>")
                    .Text("Thank you for your understanding."))
                .Build();
        }

        public static MailDatas BuildSupplyDeliverySent(
            string dateFormat,
            string keeperName,
            uint ticketNumber,            // ex: 123 → s'affiche [#123]
            float etaHour = 9f,           // heure IN-GAME estimée d'arrivée (affichée en 12h "09:00 AM")
                                          // Métadonnées d'arrivée du mail (quand il apparaît dans la boîte) :
            byte arrivalDay = 0,
            float arrivalTime = 9.0f,
            // Options UI :
            string expeditorLabel = "Coastal Trading Post",
            MailStyle? style = null,
            MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;
            // NOTE: la surcharge locale FormatTime12h() de l'ancienne version n'était jamais appelée
            // (le code appelait déjà TimeUtility.FormatTime12h) — supprimée comme code mort.
            string etaPretty = TimeUtility.FormatTime12h(etaHour).ToLowerInvariant();

            return MailBuilder.From(expeditorLabel, st)
                .Subject("Supply Delivery Sent")
                .Arrival(arrivalDay, arrivalTime)
                .Attach(files)
                .Body(t => t
                    .NewLine().Bold($"Dear {keeperName},").NewLine(2)
                    .Raw("Your order ").Bold($"[Ticket#{ticketNumber:000}]").Raw(" is ready to be dispatched.<br>")
                    .Raw("It should arrive around ").Bold(etaPretty).Raw(".<br><br>")
                    .Line("We will keep you informed of the delivery's arrival, so please stay tuned.")
                    .NewLine()
                    .Text("Best regards,"))
                .Build();
        }

        public static MailDatas BuildSupplyDeliveryCompleted(
            string dateFormat,
            string keeperName,
            uint ticketNumber,            // ex: 123 → s'affiche [#123]
                                          // Métadonnées d'arrivée du mail (quand il apparaît dans la boîte) :
            byte arrivalDay = 0,
            float arrivalTime = 9.0f,
            // Options UI :
            string expeditorLabel = "Coastal Trading Post",
            MailStyle? style = null,
            MailAttachedFile[] files = null)
        {
            var st = style ?? MailStyle.Default;

            return MailBuilder.From(expeditorLabel, st)
                .Subject("Supply Delivery Completed")
                .Arrival(arrivalDay, arrivalTime)
                .Attach(files)
                .Body(t => t
                    .NewLine().Bold($"Dear {keeperName},").NewLine(2)
                    .Raw("The delivery for your order ").Bold($"[Ticket#{ticketNumber:000}]").Raw(" has been successfully completed.<br>")
                    .Line("You will find your items in the designated storage area.")
                    .NewLine()
                    .Text("Best regards,"))
                .Build();
        }
        #endregion
    }

    /// <summary>
    /// Extensions MailText spécifiques au domaine "bulletin météo" — séparées de MailText
    /// (qui reste générique/réutilisable) pour respecter le principe de responsabilité unique.
    /// </summary>
    internal static class MailForecastExtensions
    {
        public static MailText ForecastBlock(this MailText t, string title, MailGenerator.ForecastLine f)
        {
            var st = t.Style;
            t.NewLine().Color(title, st.Accent, bold: true).NewLine();

            if (f == null)
            {
                t.Italic("Forecast unavailable.").Raw("<br>");
                return t;
            }

            string windCol = f.WindKts >= 25 ? st.Negative : (f.WindKts >= 15 ? st.Accent : st.Positive);
            string seaCol = (f.SeaState?.ToLowerInvariant().Contains("rough") ?? false) ? st.Negative : st.Body;

            t.Bullet($"{f.Period} — {f.LowC}–{f.HighC}°C");

            t.Bullet(b =>
            {
                b.Raw("Wind: ").Color($"{f.WindKts} kts", windCol);
                if (!string.IsNullOrEmpty(f.WindDir)) b.Raw(" (").Text(f.WindDir).Raw(")");
            });

            t.Bullet(b => b.Raw("Sea state: ").Color(f.SeaState, seaCol));

            if (f.ConfidencePct >= 0f) t.Bullet($"Confidence: {f.ConfidencePct:0}%");
            if (!string.IsNullOrWhiteSpace(f.Note)) t.Bullet($"Note: {f.Note}");

            return t;
        }
    }
}