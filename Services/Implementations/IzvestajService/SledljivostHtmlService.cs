using FruitSysWeb.Models.Sledljivost;
using System.Text;

namespace FruitSysWeb.Services.Implementations.IzvestajService
{
    public class SledljivostHtmlService
    {
        public byte[] GenerisiUpstreamHtml(SledljivostModel sledljivost)
        {
            var html = GenerisiHtml(sledljivost, "UPSTREAM");
            return Encoding.UTF8.GetBytes(html);
        }

        public byte[] GenerisiDownstreamHtml(SledljivostModel sledljivost)
        {
            var html = GenerisiHtml(sledljivost, "DOWNSTREAM");
            return Encoding.UTF8.GetBytes(html);
        }

        private string GenerisiHtml(SledljivostModel sledljivost, string tip)
        {
            var sb = new StringBuilder();
            var rn = sledljivost.RadniNalog;
            var generisano = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            var naslov = tip == "UPSTREAM"
                ? $"Sledljivost UPSTREAM &#x2014; Radni Nalog: {sledljivost.Sifra}"
                : $"Sledljivost DOWNSTREAM &#x2014; Paletni List: {sledljivost.Sifra}";

            sb.Append("<!DOCTYPE html>\n<html lang=\"sr\">\n<head>\n");
            sb.Append("<meta charset=\"UTF-8\">\n");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n");
            sb.Append($"<title>{naslov}</title>\n");
            sb.Append(CSS());
            sb.Append("</head>\n<body>\n");

            // Header
            sb.Append("<div class=\"header\">\n");
            sb.Append($"  <div class=\"header-title\">{naslov}</div>\n");
            sb.Append($"  <div class=\"meta\">ODETTA DOO &nbsp;|&nbsp; Generisano: {generisano}</div>\n");
            sb.Append($"  <div class=\"badge-tip\">{tip}</div>\n");
            sb.Append("</div>\n");
            sb.Append("<div class=\"content\">\n");

            // Sve PL za lookup po sifri
            var sviPl = sledljivost.PaletniListoviUlaz.Concat(sledljivost.PaletniListoviIzlaz).ToList();

            // Radni nalog info box
            if (rn != null)
            {
                var statusClass = $"status-{rn.DokumentStatus ?? 0}";
                sb.Append("<div class=\"rn-info\"><table>\n");
                sb.Append($"  <tr><td>Radni Nalog:</td><td><strong>{rn.Sifra}</strong></td></tr>\n");
                if (!string.IsNullOrEmpty(rn.LotNaloga))
                    sb.Append($"  <tr><td>LOT:</td><td><strong>{rn.LotNaloga}</strong></td></tr>\n");
                if (rn.Datum.HasValue)
                    sb.Append($"  <tr><td>Datum:</td><td>{rn.Datum.Value:dd.MM.yyyy}</td></tr>\n");
                if (!string.IsNullOrEmpty(rn.KomitentNaziv))
                    sb.Append($"  <tr><td>Kupac:</td><td>{rn.KomitentNaziv}</td></tr>\n");
                if (rn.Kolicina.HasValue)
                    sb.Append($"  <tr><td>Kolicina:</td><td>{rn.Kolicina.Value:N2} kg</td></tr>\n");
                if (rn.BrojPakovanja.HasValue)
                    sb.Append($"  <tr><td>Br. pakovanja:</td><td>{rn.BrojPakovanja.Value}</td></tr>\n");
                sb.Append("</table></div>\n");
            }

            // Timeline
            sb.Append("<div class=\"timeline\">\n");
            sb.Append($"  <div class=\"tl-step\"><div class=\"tl-icon\">&#x1F69A;</div><div class=\"tl-count\">{sledljivost.Prijemnice.Count}</div><div class=\"tl-label\">Prijemnice</div></div>\n");
            sb.Append("  <div class=\"tl-arrow\">&#x25B6;</div>\n");
            sb.Append($"  <div class=\"tl-step\"><div class=\"tl-icon\">&#x1F4E6;</div><div class=\"tl-count\">{sledljivost.PaletniListoviUlaz.Count}</div><div class=\"tl-label\">PL Ulaz</div></div>\n");
            sb.Append("  <div class=\"tl-arrow\">&#x25B6;</div>\n");
            sb.Append($"  <div class=\"tl-step\"><div class=\"tl-icon\">&#x1F3ED;</div><div class=\"tl-count\">{sledljivost.EvidencijeRada.Count}</div><div class=\"tl-label\">Evidencije Rada</div></div>\n");
            sb.Append("  <div class=\"tl-arrow\">&#x25B6;</div>\n");
            sb.Append($"  <div class=\"tl-step\"><div class=\"tl-icon\">&#x1F4E6;</div><div class=\"tl-count\">{sledljivost.PaletniListoviIzlaz.Count}</div><div class=\"tl-label\">PL Izlaz</div></div>\n");
            sb.Append("  <div class=\"tl-arrow\">&#x25B6;</div>\n");
            sb.Append($"  <div class=\"tl-step\"><div class=\"tl-icon\">&#x1F6D2;</div><div class=\"tl-count\">{sledljivost.Otpremnice.Count}</div><div class=\"tl-label\">Otpremnice</div></div>\n");
            sb.Append("</div>\n");

            // =============================================
            // SEKCIJA 1: OTPREMNICE
            // =============================================
            if (sledljivost.Otpremnice.Any())
            {
                sb.Append(OtvoriSekciju("&#x1F6D2; Otpremnice (Prodaja &#x2014; Izlaz)", sledljivost.Otpremnice.Count, "green", "otpremnice"));

                foreach (var o in sledljivost.Otpremnice)
                {
                    var metaParts = new List<string>();
                    if (o.Datum.HasValue) metaParts.Add($"&#x1F4C5; {o.Datum.Value:dd.MM.yyyy}");
                    if (!string.IsNullOrEmpty(o.KomitentNaziv)) metaParts.Add($"&#x1F3E2; {o.KomitentNaziv}");
                    if (!string.IsNullOrEmpty(o.Vozilo)) metaParts.Add($"&#x1F69B; {o.Vozilo}");

                    sb.Append(OtvoriDocCard(o.Sifra, string.Join(" &nbsp;|&nbsp; ", metaParts), $"otp_{o.ID}"));

                    // Stavke otpremnice
                    if (o.Stavke.Any())
                    {
                        sb.Append("<table><thead><tr><th>Artikal</th><th>Kolicina</th></tr></thead><tbody>\n");
                        foreach (var s in o.Stavke)
                            sb.Append($"<tr><td>{s.ArtikalNaziv}</td><td>{s.Kolicina:N2} kg</td></tr>\n");
                        sb.Append("</tbody></table>\n");
                    }

                    // Paletni listovi izlaza — samo gotova roba SA pakovanjem
                    var plZaOtp = sledljivost.PaletniListoviIzlaz
                        .Where(pl => !string.IsNullOrEmpty(pl.OtpremnicaSifra) && pl.OtpremnicaSifra == o.Sifra
                                     && pl.ArtikalMagacinID == 6 && !string.IsNullOrEmpty(pl.PakovanjeNaziv))
                        .ToList();

                    if (!plZaOtp.Any())
                        plZaOtp = sledljivost.PaletniListoviIzlaz
                            .Where(pl => pl.ArtikalMagacinID == 6 && !string.IsNullOrEmpty(pl.PakovanjeNaziv))
                            .ToList();

                    if (plZaOtp.Any())
                    {
                        sb.Append($"<div class=\"sub-title\">Paletni Listovi ({plZaOtp.Count}):</div>\n");
                        foreach (var pl in plZaOtp)
                            sb.Append(RenderPaletniListKartica(pl, $"pl_otp_{o.ID}_{pl.ID}", sviPl));
                    }

                    sb.Append(ZatvoriDocCard());
                }

                sb.Append(ZatvoriSekciju());
            }

            // =============================================
            // SEKCIJA 2: EVIDENCIJE RADA
            // =============================================
            if (sledljivost.EvidencijeRada.Any())
            {
                sb.Append(OtvoriSekciju("&#x1F3ED; Evidencije Rada (Proizvodnja)", sledljivost.EvidencijeRada.Count, "purple", "evidencije"));

                foreach (var e in sledljivost.EvidencijeRada)
                {
                    var metaParts = new List<string>();
                    if (e.Datum.HasValue) metaParts.Add($"&#x1F4C5; {e.Datum.Value:dd.MM.yyyy}");
                    if (e.Smena.HasValue) metaParts.Add($"Smena {e.Smena.Value}");
                    if (e.BrojRadnihSati.HasValue) metaParts.Add($"&#x23F1; {e.BrojRadnihSati.Value:N1}h");
                    if (!string.IsNullOrEmpty(e.SmenskiIzvestajSifra)) metaParts.Add($"SI: {e.SmenskiIzvestajSifra}");

                    sb.Append(OtvoriDocCard(e.Sifra, string.Join(" &nbsp;|&nbsp; ", metaParts), $"evid_{e.ID}"));

                    var plZaEvidenciju = sledljivost.PaletniListoviUlaz
                        .Where(pl => !string.IsNullOrEmpty(pl.EvidencijaRadaSifra) && pl.EvidencijaRadaSifra == e.Sifra)
                        .ToList();

                    var plKorisceni = sledljivost.PaletniListoviUlaz
                        .Where(pl => pl.KoriscenUEvidencijama?.Contains(e.Sifra) == true && !plZaEvidenciju.Contains(pl))
                        .ToList();
                    plZaEvidenciju.AddRange(plKorisceni);

                    if (e.UtroseniPaletniListoviIDs?.Any() == true)
                    {
                        var plUtroseni = sledljivost.PaletniListoviUlaz
                            .Where(pl => e.UtroseniPaletniListoviIDs.Contains(pl.ID) && !plZaEvidenciju.Contains(pl))
                            .ToList();
                        plZaEvidenciju.AddRange(plUtroseni);
                    }

                    if (plZaEvidenciju.Any())
                    {
                        sb.Append($"<div class=\"sub-title\">Utroseni Paletni Listovi ({plZaEvidenciju.Count}):</div>\n");
                        foreach (var pl in plZaEvidenciju)
                            sb.Append(RenderPaletniListKartica(pl, $"pl_evid_{e.ID}_{pl.ID}", sviPl));
                    }
                    else
                    {
                        sb.Append("<div class=\"no-data\">Nema evidentiranih paletnih listova</div>\n");
                    }

                    sb.Append(ZatvoriDocCard());
                }

                sb.Append(ZatvoriSekciju());
            }

            // =============================================
            // SEKCIJA 3: PRIJEMNICE
            // =============================================
            if (sledljivost.Prijemnice.Any())
            {
                sb.Append(OtvoriSekciju("&#x1F69A; Prijemnice (Nabavka &#x2014; Ulaz sirovine)", sledljivost.Prijemnice.Count, "blue", "prijemnice"));

                foreach (var p in sledljivost.Prijemnice)
                {
                    var metaParts = new List<string>();
                    if (p.Datum.HasValue) metaParts.Add($"&#x1F4C5; {p.Datum.Value:dd.MM.yyyy}");
                    if (!string.IsNullOrEmpty(p.KomitentNaziv)) metaParts.Add($"&#x1F3E2; {p.KomitentNaziv}");
                    if (!string.IsNullOrEmpty(p.Vozilo)) metaParts.Add($"&#x1F69B; {p.Vozilo}");

                    sb.Append(OtvoriDocCard(p.Sifra, string.Join(" &nbsp;|&nbsp; ", metaParts), $"prij_{p.ID}"));

                    // Stavke prijemnice
                    if (p.Stavke.Any())
                    {
                        sb.Append("<table><thead><tr><th>Artikal</th><th>Kolicina</th></tr></thead><tbody>\n");
                        foreach (var s in p.Stavke)
                            sb.Append($"<tr><td>{s.ArtikalNaziv}</td><td>{s.Kolicina:N2} kg</td></tr>\n");
                        sb.Append("</tbody></table>\n");
                    }

                    // Paletni listovi ulaza (samo sirovine/ambalaza, bez gotove robe)
                    var plZaPrijemnicu = sledljivost.PaletniListoviUlaz
                        .Where(pl => !string.IsNullOrEmpty(pl.PrijemnicaSifra) && pl.PrijemnicaSifra == p.Sifra
                                     && pl.ArtikalMagacinID != 6)
                        .ToList();

                    if (plZaPrijemnicu.Any())
                    {
                        sb.Append($"<div class=\"sub-title\">Paletni Listovi ({plZaPrijemnicu.Count}):</div>\n");
                        foreach (var pl in plZaPrijemnicu)
                            sb.Append(RenderPaletniListKartica(pl, $"pl_prij_{pl.ID}", sviPl));
                    }

                    sb.Append(ZatvoriDocCard());
                }

                sb.Append(ZatvoriSekciju());
            }

            sb.Append("</div>\n");
            sb.Append($"<div class=\"footer\">ODETTA DOO &nbsp;|&nbsp; Sledljivost robe &nbsp;|&nbsp; {generisano} &nbsp;|&nbsp; FruitSysWeb</div>\n");
            sb.Append(JS());
            sb.Append("</body>\n</html>");

            return sb.ToString();
        }

        // =============================================
        // Paletni List kartica — klik otvara hover podatke
        // =============================================
        private string RenderPaletniListKartica(PaletniListDetalji pl, string cardId, List<PaletniListDetalji> _sviPl)
        {
            var sb = new StringBuilder();
            var tipClass = pl.ArtikalMagacinID == 6 ? "gotov" : (pl.PrijemnicaStavkaID.HasValue ? "nabavka" : "polu");

            sb.Append($"<div class=\"pl-kartica {tipClass}\">\n");
            sb.Append($"  <div class=\"pl-header\" onclick=\"togglePl('{cardId}')\">\n");
            sb.Append($"    <div>\n");
            sb.Append($"      <span class=\"pl-sifra\">&#x1F4E6; {pl.Sifra}</span>\n");
            sb.Append($"      <span class=\"pl-tip-badge\">{pl.TipPaletnogLista}</span>\n");
            sb.Append($"    </div>\n");
            sb.Append($"    <div class=\"pl-summary\">{pl.ArtikalNaziv}{(pl.Tezina.HasValue ? $" &nbsp;&#x2014;&nbsp; {pl.Tezina.Value:N2} kg" : "")}</div>\n");
            sb.Append($"    <span class=\"pl-chevron\" id=\"plchev_{cardId}\">&#x25B6;</span>\n");
            sb.Append($"  </div>\n");

            // Expandable detalji (hover podaci)
            sb.Append($"  <div class=\"pl-detalji\" id=\"pld_{cardId}\">\n");
            sb.Append($"    <table>\n");
            sb.Append($"      <tr><td class=\"lbl\">Paletni List:</td><td><strong>{pl.Sifra}</strong></td></tr>\n");
            sb.Append($"      <tr><td class=\"lbl\">Tip:</td><td>{pl.TipPaletnogLista}</td></tr>\n");
            sb.Append($"      <tr><td class=\"lbl\">Artikal:</td><td>{pl.ArtikalNaziv}</td></tr>\n");
            if (pl.Tezina.HasValue)
                sb.Append($"      <tr><td class=\"lbl\">Tezina:</td><td>{pl.Tezina.Value:N2} kg</td></tr>\n");
            if (!string.IsNullOrEmpty(pl.PakovanjeNaziv))
                sb.Append($"      <tr><td class=\"lbl\">Pakovanje:</td><td>{pl.PakovanjeNaziv}</td></tr>\n");
            if (!string.IsNullOrEmpty(pl.AmbalazaNaziv))
                sb.Append($"      <tr><td class=\"lbl\">Ambalaza:</td><td>{pl.AmbalazaNaziv}</td></tr>\n");
            if (pl.DatumKreiranja.HasValue)
                sb.Append($"      <tr><td class=\"lbl\">Datum:</td><td>{pl.DatumKreiranja.Value:dd.MM.yyyy}</td></tr>\n");

            // Upstream veze
            bool hasUpstream = !string.IsNullOrEmpty(pl.PrijemnicaSifra) || !string.IsNullOrEmpty(pl.LotDobavljaca) || !string.IsNullOrEmpty(pl.OtpremnicaDobavljaca);
            if (hasUpstream)
            {
                sb.Append($"      <tr><td colspan=\"2\" class=\"section-lbl\">&#x1F53C; UPSTREAM (Nabavka)</td></tr>\n");
                if (!string.IsNullOrEmpty(pl.PrijemnicaSifra))
                    sb.Append($"      <tr><td class=\"lbl\">Prijemnica:</td><td>{pl.PrijemnicaSifra}</td></tr>\n");
                if (!string.IsNullOrEmpty(pl.LotDobavljaca))
                    sb.Append($"      <tr><td class=\"lbl\">LOT dobavljaca:</td><td>{pl.LotDobavljaca}</td></tr>\n");
                if (!string.IsNullOrEmpty(pl.OtpremnicaDobavljaca))
                    sb.Append($"      <tr><td class=\"lbl\">Otpremnica dobavljaca:</td><td>{pl.OtpremnicaDobavljaca}</td></tr>\n");
            }

            // Proizvodnja veze
            bool hasProzvodnja = !string.IsNullOrEmpty(pl.EvidencijaRadaSifra) || !string.IsNullOrEmpty(pl.SmenskiIzvestajSifra) || !string.IsNullOrEmpty(pl.RadniNalogSifra)
                                 || pl.KoriscenUEvidencijama?.Any() == true || pl.KoriscenUSmenama?.Any() == true || pl.KoriscenURadnimNalozima?.Any() == true;
            if (hasProzvodnja)
            {
                sb.Append($"      <tr><td colspan=\"2\" class=\"section-lbl\">&#x1F3ED; Proizvodnja</td></tr>\n");
                if (!string.IsNullOrEmpty(pl.EvidencijaRadaSifra))
                    sb.Append($"      <tr><td class=\"lbl\">Evidencija Rada:</td><td>{pl.EvidencijaRadaSifra}</td></tr>\n");
                if (!string.IsNullOrEmpty(pl.SmenskiIzvestajSifra))
                    sb.Append($"      <tr><td class=\"lbl\">Smenski Izvestaj:</td><td>{pl.SmenskiIzvestajSifra}</td></tr>\n");
                if (!string.IsNullOrEmpty(pl.RadniNalogSifra))
                    sb.Append($"      <tr><td class=\"lbl\">Radni Nalog:</td><td>{pl.RadniNalogSifra}</td></tr>\n");
                if (pl.KoriscenUEvidencijama?.Any() == true)
                    sb.Append($"      <tr><td class=\"lbl\">Koriscen u evidencijama:</td><td>{string.Join(", ", pl.KoriscenUEvidencijama)}</td></tr>\n");
                if (pl.KoriscenUSmenama?.Any() == true)
                    sb.Append($"      <tr><td class=\"lbl\">Koriscen u smenama:</td><td>{string.Join(", ", pl.KoriscenUSmenama)}</td></tr>\n");
                if (pl.KoriscenURadnimNalozima?.Any() == true)
                    sb.Append($"      <tr><td class=\"lbl\">Koriscen u RN:</td><td>{string.Join(", ", pl.KoriscenURadnimNalozima)}</td></tr>\n");
            }

            // Downstream veze
            if (!string.IsNullOrEmpty(pl.OtpremnicaSifra))
            {
                sb.Append($"      <tr><td colspan=\"2\" class=\"section-lbl\">&#x1F53D; DOWNSTREAM (Prodaja)</td></tr>\n");
                sb.Append($"      <tr><td class=\"lbl\">Otpremnica:</td><td>{pl.OtpremnicaSifra}</td></tr>\n");
            }

            // Povezani paletni listovi — trazimo detalje iz vec ucitanih PL
            if (pl.PovezaniPaletniListoviSifre?.Any() == true)
            {
                sb.Append($"      <tr><td colspan=\"2\" class=\"section-lbl\">&#x1F517; Povezani paletni listovi ({pl.PovezaniPaletniListoviSifre.Count})</td></tr>\n");
                foreach (var plSifra in pl.PovezaniPaletniListoviSifre)
                {
                    var povPl = _sviPl.FirstOrDefault(x => x.Sifra == plSifra);
                    if (povPl != null)
                    {
                        var opisPl = $"{plSifra}{(string.IsNullOrEmpty(povPl.ArtikalNaziv) ? "" : $" &#x2014; {povPl.ArtikalNaziv}")}{(string.IsNullOrEmpty(povPl.KomitentNaziv) ? "" : $" ({povPl.KomitentNaziv})")}";
                        sb.Append($"      <tr><td class=\"lbl\"></td><td>{opisPl}</td></tr>\n");
                    }
                    else
                    {
                        sb.Append($"      <tr><td class=\"lbl\"></td><td>{plSifra}</td></tr>\n");
                    }
                }
            }

            sb.Append($"    </table>\n");
            sb.Append($"  </div>\n");
            sb.Append($"</div>\n");

            return sb.ToString();
        }

        private string OtvoriSekciju(string naslov, int count, string boja, string sectionId)
        {
            var bojaClass = boja switch { "green" => "green", "orange" => "orange", "purple" => "purple", "teal" => "teal", _ => "" };
            var badgeClass = boja switch { "green" => "green", "orange" => "orange", "purple" => "purple", _ => "" };
            return $"<div class=\"section\">\n  <div class=\"section-header {bojaClass}\" onclick=\"toggleSection('{sectionId}')\">\n    <div class=\"section-title\">{naslov} <span class=\"badge {badgeClass}\">{count}</span></div>\n    <span class=\"chevron\" id=\"chev_{sectionId}\">&#x25B6;</span>\n  </div>\n  <div class=\"section-body\" id=\"sb_{sectionId}\">\n";
        }

        private string ZatvoriSekciju() => "</div></div>\n";

        private string OtvoriDocCard(string? sifra, string meta, string cardId)
        {
            return $"<div class=\"doc-card\">\n  <div class=\"doc-card-header\" onclick=\"toggleDoc('{cardId}')\">\n    <div><div class=\"doc-sifra\">&#x1F4C4; {sifra}</div><div class=\"doc-meta\">{meta}</div></div>\n  </div>\n  <div class=\"doc-card-body\" id=\"db_{cardId}\">\n";
        }

        private string ZatvoriDocCard() => "</div></div>\n";

        private static string CSS() => @"<style>
* { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: 'Segoe UI', Arial, sans-serif; background: #f4f6f9; color: #2d3748; font-size: 14px; }
.header { background: linear-gradient(135deg, #1a365d 0%, #2d6a4f 100%); color: white; padding: 28px 40px; }
.header-title { font-size: 22px; font-weight: 700; margin-bottom: 6px; }
.meta { font-size: 12px; opacity: 0.85; }
.badge-tip { display: inline-block; background: rgba(255,255,255,0.2); border-radius: 20px; padding: 3px 12px; font-size: 11px; font-weight: 600; margin-top: 8px; }
.content { max-width: 980px; margin: 0 auto; padding: 30px 20px; }
.section { margin-bottom: 20px; }
.section-header { background: white; border-radius: 10px 10px 0 0; padding: 14px 20px; cursor: pointer; display: flex; align-items: center; justify-content: space-between; box-shadow: 0 2px 6px rgba(0,0,0,0.07); border-left: 5px solid #3182ce; user-select: none; }
.section-header:hover { background: #ebf8ff; }
.section-header.green { border-left-color: #38a169; }
.section-header.orange { border-left-color: #dd6b20; }
.section-header.purple { border-left-color: #805ad5; }
.section-header.teal { border-left-color: #319795; }
.section-title { font-weight: 700; font-size: 15px; display: flex; align-items: center; gap: 10px; }
.badge { display: inline-block; background: #3182ce; color: white; border-radius: 20px; padding: 2px 10px; font-size: 11px; font-weight: 600; }
.badge.green { background: #38a169; }
.badge.orange { background: #dd6b20; }
.badge.purple { background: #805ad5; }
.chevron { font-size: 14px; transition: transform 0.2s; }
.chevron.open { transform: rotate(90deg); }
.section-body { background: white; border-radius: 0 0 10px 10px; padding: 0 20px 16px 20px; box-shadow: 0 2px 6px rgba(0,0,0,0.07); display: none; }
.section-body.open { display: block; }
.doc-card { border: 1px solid #e2e8f0; border-radius: 8px; margin: 12px 0; overflow: hidden; }
.doc-card-header { background: #f7fafc; padding: 12px 16px; cursor: pointer; display: flex; align-items: center; justify-content: space-between; }
.doc-card-header:hover { background: #ebf8ff; }
.doc-card-body { padding: 12px 16px; display: none; border-top: 1px solid #e2e8f0; }
.doc-card-body.open { display: block; }
.doc-sifra { font-weight: 700; font-size: 14px; }
.doc-meta { font-size: 12px; color: #718096; margin-top: 3px; }
.sub-title { font-size: 12px; font-weight: 600; color: #4a5568; margin: 12px 0 6px 0; padding-top: 8px; border-top: 1px dashed #e2e8f0; }
.no-data { font-size: 12px; color: #a0aec0; font-style: italic; margin-top: 8px; }
table { width: 100%; border-collapse: collapse; font-size: 13px; margin-top: 8px; }
th { background: #edf2f7; text-align: left; padding: 7px 10px; font-weight: 600; color: #4a5568; border-bottom: 2px solid #cbd5e0; }
td { padding: 6px 10px; border-bottom: 1px solid #e2e8f0; }
tr:last-child td { border-bottom: none; }
tr:hover td { background: #f7fafc; }
.rn-info { background: #ebf8ff; border-radius: 10px; padding: 18px 24px; margin-bottom: 24px; border-left: 5px solid #3182ce; }
.rn-info table { margin-top: 0; }
.rn-info td { padding: 5px 10px; border: none; }
.rn-info td:first-child { font-weight: 600; color: #4a5568; width: 160px; }
.status-badge { display: inline-block; border-radius: 12px; padding: 2px 10px; font-size: 11px; font-weight: 600; }
.status-2 { background: #bee3f8; color: #2b6cb0; }
.status-3 { background: #c6f6d5; color: #276749; }
.status-4 { background: #fed7d7; color: #9b2c2c; }
.status-0, .status- { background: #e2e8f0; color: #4a5568; }
.footer { text-align: center; font-size: 11px; color: #a0aec0; padding: 20px; margin-top: 10px; }
.timeline { display: flex; align-items: center; gap: 0; margin-bottom: 24px; flex-wrap: wrap; }
.tl-step { flex: 1; min-width: 110px; text-align: center; padding: 12px 6px; background: white; border-radius: 8px; box-shadow: 0 2px 6px rgba(0,0,0,0.07); }
.tl-icon { font-size: 22px; }
.tl-label { font-size: 10px; font-weight: 600; color: #718096; margin-top: 3px; }
.tl-count { font-size: 18px; font-weight: 700; color: #2d3748; }
.tl-arrow { font-size: 18px; color: #cbd5e0; padding: 0 3px; }
/* Paletni List kartica */
.pl-kartica { border: 1px solid #e2e8f0; border-radius: 7px; margin: 6px 0; overflow: hidden; }
.pl-kartica.nabavka { border-left: 3px solid #fc8181; }
.pl-kartica.gotov { border-left: 3px solid #68d391; }
.pl-kartica.polu { border-left: 3px solid #f6ad55; }
.pl-header { background: #fafafa; padding: 8px 12px; cursor: pointer; display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.pl-header:hover { background: #f0fff4; }
.pl-sifra { font-weight: 700; font-size: 13px; }
.pl-tip-badge { display: inline-block; background: #e2e8f0; color: #4a5568; border-radius: 10px; padding: 1px 8px; font-size: 10px; font-weight: 600; margin-left: 6px; }
.pl-summary { font-size: 12px; color: #718096; flex: 1; padding: 0 8px; }
.pl-chevron { font-size: 12px; color: #a0aec0; transition: transform 0.2s; }
.pl-chevron.open { transform: rotate(90deg); }
.pl-detalji { display: none; padding: 10px 14px; background: #f7fafc; border-top: 1px solid #e2e8f0; }
.pl-detalji.open { display: block; }
.pl-detalji table { margin-top: 0; }
.pl-detalji td { padding: 4px 8px; border: none; font-size: 12px; }
.pl-detalji td.lbl { font-weight: 600; color: #718096; width: 180px; }
.pl-detalji td.section-lbl { font-weight: 700; color: #4a5568; padding-top: 8px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; }
@media print {
  .section-body { display: block !important; }
  .doc-card-body { display: block !important; }
  .pl-detalji { display: block !important; }
  .chevron, .pl-chevron { display: none; }
  body { background: white; }
}
</style>
";

        private static string JS() => @"<script>
function toggleSection(id) {
  var body = document.getElementById('sb_' + id);
  var chev = document.getElementById('chev_' + id);
  if (body.classList.contains('open')) {
    body.classList.remove('open');
    chev.classList.remove('open');
  } else {
    body.classList.add('open');
    chev.classList.add('open');
  }
}
function toggleDoc(id) {
  var body = document.getElementById('db_' + id);
  if (body.classList.contains('open')) {
    body.classList.remove('open');
  } else {
    body.classList.add('open');
  }
}
function togglePl(id) {
  var detalji = document.getElementById('pld_' + id);
  var chev = document.getElementById('plchev_' + id);
  if (detalji.classList.contains('open')) {
    detalji.classList.remove('open');
    chev.classList.remove('open');
  } else {
    detalji.classList.add('open');
    chev.classList.add('open');
  }
}
document.addEventListener('DOMContentLoaded', function() {
  var allSections = document.querySelectorAll('.section-body');
  allSections.forEach(function(s) { s.classList.add('open'); });
  var allChevs = document.querySelectorAll('.chevron');
  allChevs.forEach(function(c) { c.classList.add('open'); });
});
</script>
";
    }
}
