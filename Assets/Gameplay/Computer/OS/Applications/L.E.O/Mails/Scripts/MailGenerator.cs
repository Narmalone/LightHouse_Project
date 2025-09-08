using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using LightHouse.Game.Computer.LEO.Mails;
using LightHouse.Game.DayNightSystem;

public static class MailGenerator
{
    // ---------- STYLE ----------
    [Serializable]
    public struct MailStyle
    {
        public string Primary;   // titres
        public string Accent;    // sous-titres / infos
        public string Positive;  // ok/gains
        public string Negative;  // erreurs/alertes
        public string Body;      // texte principal

        public int TitlePct;   // % taille titre
        public int BodyPct;    // % taille corps
        public float LineHeight; // % line-height

        public static MailStyle Default => new MailStyle
        {
            Primary = "#1E88E5",
            Accent = "#8EACBB",
            Positive = "#00C853",
            Negative = "#FF5252",
            Body = "#E0E0E0",
            TitlePct = 150,
            BodyPct = 100,
            LineHeight = 110f
        };
    }

    // ---------- TYPES D’AIDE ----------
    [Serializable]
    public class ForecastLine
    {
        public string Period;
        public int LowC;
        public int HighC;
        public int WindKts;
        public string SeaState;
        public string Note;

        public ForecastLine(string period, int lowC, int highC, int windKts, string seaState, string note = "")
        {
            Period = period; LowC = lowC; HighC = highC; WindKts = windKts; SeaState = seaState; Note = note;
        }
    }
/*
    [Serializable]
    public class SupplyItem
    {
        public string Name;
        public int Quantity;
        public string Unit;   // "pcs", "kg", "L"…
        public string Notes;

        public SupplyItem(string name, int quantity, string unit = "", string notes = "")
        {
            Name = name; Quantity = quantity; Unit = unit; Notes = notes;
        }
    }*/

    // ---------- HELPERS ----------
    private static string EscapeRT(string s) => s?
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;");

    private static string Money(float amount, string currencySymbol = "$")
        => string.Format(CultureInfo.InvariantCulture, "{0}{1:N0}", currencySymbol, amount);

    private static string Header(string subjectLeft, string fromRight, MailStyle st)
    {
        return
$@"<line-height={st.LineHeight}%><size={st.BodyPct}%><color={st.Body}>
<align=left><size={st.TitlePct}%><b><color={st.Primary}>{EscapeRT(subjectLeft)}</color></b></size></align>
<align=right><i><color={st.Accent}>{EscapeRT(fromRight)}</color></i></align>
<align=left>";
    }

    private static string Footer(string signature, MailStyle st)
    {
        return $@"<br><br>Respectfully,<br>{EscapeRT(signature)}</color>";
    }

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

    // =====================================================================================
    // NIGHTWATCH
    // =====================================================================================
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
        string subject = $"Nightwatch Report – {dateFormat}";
        string head = Header(subject, $"From : {expeditorLabel}", st);

        string boatsErrColor = boatsErrors > 0 ? st.Negative : st.Accent;
        string buoysErrColor = buoysErrors > 0 ? st.Negative : st.Accent;
        string body =
$@"<br><b>Dear {EscapeRT(keeperName)},</b><br><br>
Please find below the summary of your night watch:<br><br>

<b>Boats:</b><br>
• Correct reports: <b><color={st.Positive}>{boatsCorrect}</color></b><br>
• Errors: <b><color={boatsErrColor}>{boatsErrors}</color></b><br><br>

<b>Buoys:</b><br>
• Correct nominal buoys: <b><color={st.Positive}>{buoysNominal}</color></b><br>
• Correct defective buoys: <b><color={st.Positive}>{buoysDefective}</color></b><br>
• Errors: <b><color={buoysErrColor}>{buoysErrors}</color></b><br><br>

<b>Total earnings: <color={st.Positive}>{Money(totalEarnings, currencySymbol)}</color></b><br><br>

<b>Captain’s Note:</b><br>
<i>{EscapeRT(captainsNote)}</i>";

        string foot = Footer(expeditorLabel, st);
        string message = head + body + foot;
        return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, message, files);
    }

    // Exemple minimal
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

    // =====================================================================================
    // WEATHER
    // =====================================================================================
    public static MailDatas GenerateMailFromWeatherTemplate(
        DateTime date,
        string portName,
        IEnumerable<ForecastLine> forecast,
        string advisory,
        // Métadonnées MailDatas :
        byte arrivalDay,
        float arrivalTime,
        // Options :
        string expeditorLabel = "Harbor Master’s Office",
        MailStyle? style = null,
        MailAttachedFile[] files = null)
    {
        var st = style ?? MailStyle.Default;
        string subject = $"Weather Briefing – {portName} – {date:yyyy-MM-dd}";
        string head = Header(subject, $"From : {expeditorLabel}", st);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("<br><b>Dear Captain,</b><br><br>");
        sb.AppendLine("Please find the marine weather summary:<br>");

        foreach (var f in forecast)
        {
            string windColor = f.WindKts >= 25 ? st.Negative : (f.WindKts >= 15 ? st.Accent : st.Positive);
            string seaColor = (f.SeaState?.ToLowerInvariant().Contains("rough") ?? false) ? st.Negative : st.Body;

            sb.AppendLine($"<br><b><color={st.Primary}>{EscapeRT(f.Period)}</color></b><br>");
            sb.AppendLine($"• Temp: {f.LowC}–{f.HighC}°C<br>");
            sb.AppendLine($"• Wind: <color={windColor}>{f.WindKts} kts</color><br>");
            sb.AppendLine($"• Sea state: <color={seaColor}>{EscapeRT(f.SeaState)}</color><br>");
            if (!string.IsNullOrWhiteSpace(f.Note))
                sb.AppendLine($"• Note: {EscapeRT(f.Note)}<br>");
        }

        string advColor = string.IsNullOrWhiteSpace(advisory) || advisory.Equals("No special advisories.", StringComparison.OrdinalIgnoreCase)
            ? st.Positive : st.Negative;

        sb.AppendLine($"<br><b>Advisory:</b> <i><color={advColor}>{EscapeRT(advisory)}</color></i>");

        string foot = Footer(expeditorLabel, st);
        string message = head + sb.ToString() + foot;

        return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, message, files);
    }

    public static MailDatas GenerateMailFromWeatherTemplate()
    {
        var sample = new List<ForecastLine> {
            new ForecastLine("Tonight", 14, 18, 12, "Calm to slight"),
            new ForecastLine("Tomorrow Morning", 15, 20, 18, "Moderate", "Light showers possible"),
            new ForecastLine("Tomorrow Afternoon", 18, 23, 26, "Rough", "Gusts up to 32 kts")
        };
        return GenerateMailFromWeatherTemplate(DateTime.Today, "Greyhaven", sample, "Small craft advisory from 14:00–22:00.", 3, 10.5f);
    }

    // =====================================================================================
    // SUPPLIES / REQUISITIONS
    // =====================================================================================
    public static MailDatas GenerateMailFromSuppliesTemplate(
        string vesselName,
        string periodLabel,
        IEnumerable<SupplyItem> items,
        string additionalNotes,
        // Métadonnées MailDatas :
        byte arrivalDay,
        float arrivalTime,
        // Options :
        string currencySymbol = "$",
        float estimatedBudget = 0f,
        string expeditorLabel = "Quartermaster’s Office",
        MailStyle? style = null,
        MailAttachedFile[] files = null)
    {
        var st = style ?? MailStyle.Default;
        string subject = $"Supplies Request – {vesselName} – {periodLabel}";
        string head = Header(subject, $"From : {expeditorLabel}", st);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<br><b>Dear {EscapeRT(vesselName)} crew,</b><br><br>");
        sb.AppendLine("Please review the supplies list below:<br>");

        foreach (var it in items)
        {
          /*  string qty = string.IsNullOrEmpty(it.Unit) ? it.Quantity.ToString() : $"{it.Quantity} {it.Unit}";
            sb.Append("• ");
            sb.Append($"<b>{EscapeRT(it.Name)}</b> — {EscapeRT(qty)}");
            if (!string.IsNullOrWhiteSpace(it.Notes))
                sb.Append($@" <i><color={st.Accent}>({EscapeRT(it.Notes)})</color></i>");
            sb.Append("<br>");*/
        }

        if (estimatedBudget > 0f)
            sb.AppendLine($"<br><b>Estimated budget:</b> <color={st.Primary}>{Money(estimatedBudget, currencySymbol)}</color>");

        if (!string.IsNullOrWhiteSpace(additionalNotes))
            sb.AppendLine($"<br><b>Notes:</b> <i>{EscapeRT(additionalNotes)}</i>");

        string foot = Footer(expeditorLabel, st);
        string message = head + sb.ToString() + foot;

        return MakeMailDatas(expeditorLabel, subject, arrivalDay, arrivalTime, message, files);
    }

    public static MailDatas GenerateMailFromSuppliesTemplate()
    {
        var items = new List<SupplyItem> {
          /*  new SupplyItem("Diesel", 1200, "L", "Priority"),
            new SupplyItem("First-aid kits", 5, "pcs"),
            new SupplyItem("Flares (SOLAS)", 24, "pcs", "Expiring next month"),
            new SupplyItem("Fresh water", 500, "L")*/
        };
        return GenerateMailFromSuppliesTemplate("SS Nightjar", "Week 38", items, "Please confirm substitutions if stock is limited.", 4, 8.0f, "$", 3200f);
    }
}
