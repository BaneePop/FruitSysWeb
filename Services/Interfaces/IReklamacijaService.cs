using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IReklamacijaService
    {
        Task<List<ReklamacijaModel>> UcitajSveReklamacije();
        Task<List<ReklamacijaModel>> FilterirajReklamacije(ReklamacijaFilterModel filter);
        Task<ReklamacijaModel?> UcitajReklamaciju(string id);
        Task<ReklamacijaModel> SacuvajReklamaciju(ReklamacijaModel reklamacija);
        Task ObrisiReklamaciju(string id);

        Task<ReklamacijaDokumentModel> DodajDokument(string reklamacijaId, string tipDokumenta,
            string naziv, Stream sadrzaj, string ekstenzija);
        Task ObrisiDokument(string reklamacijaId, string dokumentId);
        Task<(Stream Sadrzaj, string Naziv, string ContentType)> PreuzmiDokument(string reklamacijaId, string dokumentId);

        Task<List<string>> UcitajSveKupce();
        Task<List<string>> UcitajSveVrsteVoca();
    }
}
