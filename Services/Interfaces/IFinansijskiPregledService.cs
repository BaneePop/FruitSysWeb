using FruitSysWeb.Models;

namespace FruitSysWeb.Services.Interfaces;

/// <summary>
/// Interface za Finansijski Pregled servis
/// </summary>
public interface IFinansijskiPregledService
{
    /// <summary>
    /// Učitava robu na zalihama sa nabavkom, prodajom i lagerom grupisano po vrsti voća
    /// </summary>
    /// <param name="odDatum">Početni datum (01.06.2025)</param>
    /// <param name="doDatum">Krajnji datum (danas)</param>
    /// <returns>Lista robe na zalihama po vrstama</returns>
    Task<List<RobaZalihaModel>> UcitajRobuNaZalihama(DateTime odDatum, DateTime doDatum);

    /// <summary>
    /// Učitava obračun otkupa po dobavljačima
    /// </summary>
    /// <param name="odDatum">Početni datum (01.06.2025)</param>
    /// <param name="doDatum">Krajnji datum (danas)</param>
    /// <returns>Lista obračuna po dobavljačima</returns>
    Task<List<ObracunOtkupaModel>> UcitajObracunOtkupa(DateTime odDatum, DateTime doDatum);

    /// <summary>
    /// Učitava kompletan obračun - sva roba + troškovi
    /// </summary>
    /// <param name="odDatum">Početni datum (01.06.2025)</param>
    /// <param name="doDatum">Krajnji datum (danas)</param>
    /// <returns>Kompletan obračun</returns>
    Task<ObracunSvaRobaModel> UcitajObracunSvaRoba(DateTime odDatum, DateTime doDatum);

    /// <summary>
    /// Računa prosečnu cenu robe na osnovu zadnjih 3 kalkulacije
    /// Ako nema kalkulacija, koristi nabavnu cenu sirovine
    /// </summary>
    /// <param name="artikalId">ID artikla</param>
    /// <returns>Prosečna cena</returns>
    Task<decimal> IzracunajProsecnuCenu(int artikalId);
}
