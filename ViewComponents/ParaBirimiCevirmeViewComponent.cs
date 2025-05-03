using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MuhasebeStokWebApp.Services.Interfaces;

namespace MuhasebeStokWebApp.ViewComponents
{
    public class ParaBirimiCevirmeViewComponent : ViewComponent
    {
        private readonly IDovizKuruService _dovizKuruService;

        public ParaBirimiCevirmeViewComponent(IDovizKuruService dovizKuruService)
        {
            _dovizKuruService = dovizKuruService;
        }

        public async Task<IViewComponentResult> InvokeAsync(decimal tutar, string kaynakKod, string hedefKod)
        {
            try
            {
                // Aynı para birimi ise doğrudan tutarı döndür
                if (kaynakKod == hedefKod)
                {
                    return View("Default", tutar.ToString("N2"));
                }

                // Para birimi dönüşümünü yap
                var cevirilmisTutar = await _dovizKuruService.CevirmeTutarByKodAsync(tutar, kaynakKod, hedefKod);
                
                return View("Default", cevirilmisTutar.ToString("N2"));
            }
            catch (Exception ex)
            {
                // Hata durumunda en son bilinen kuru almayı dene
                try
                {
                    var sonKur = await _dovizKuruService.GetGuncelKurAsync(kaynakKod, hedefKod);
                    if (sonKur > 0)
                    {
                        var cevirilmisTutar = tutar * sonKur;
                        return View("Default", cevirilmisTutar.ToString("N2"));
                    }
                }
                catch
                {
                    // Güncel kur da alınamazsa sessizce devam et
                }
                
                // Tüm seçenekler başarısız olursa, hesaplama yapılamadığını belirt
                return View("Default", "-");
            }
        }
    }
} 