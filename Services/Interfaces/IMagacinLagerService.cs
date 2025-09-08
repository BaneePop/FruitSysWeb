using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FruitSysWeb.Models;
using FruitSysWeb.Services.Models.Requests;

namespace FruitSysWeb.Services.Interfaces
{
    public interface IMagacinLagerService
    {
        Task<List<MagacinLagerModel>> UcitajLagerStanje();
        Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(FilterRequest filterRequest);
        Task<List<MagacinLagerModel>> UcitajLagerStanjeSaFilterima(string filter); // Overload za string filter
        Task<List<RadniNalogLagerModel>> UcitajRadneNalogeLager();
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(long artikalId);
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoArtiklu(int artikalId); // Overload za int
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoTipu(int tip);
        Task<List<MagacinLagerModel>> UcitajGotoveRobe();
        Task<List<MagacinLagerModel>> UcitajSirovine();
        Task<List<MagacinLagerModel>> UcitajAmbalaze();
        Task<decimal> UcitajUkupnuVrednostLager();
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoLotu(string lot);
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoRokuVazenja(DateTime rokVazenja);
        Task<List<MagacinLagerModel>> UcitajLagerStanjePoRokuVazenja(); // Overload bez parametara
        Task<List<RadniNalogLagerModel>> UcitajOtvoreneRadneNaloge();
        Task<List<RadniNalogLagerModel>> UcitajRadneNalogePoStatusu(int status);
        Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(decimal minKolicina = 10);
        Task<List<MagacinLagerModel>> UcitajArtikleIspodMinimuma(); // Overload bez parametara
        Task<Dictionary<string, decimal>> UcitajStatistikeLagera();
    }
}
