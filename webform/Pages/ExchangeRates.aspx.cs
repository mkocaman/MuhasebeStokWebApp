using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using MaOaKApp.Services;

namespace MaOaKApp.Pages
{
    public partial class ExchangeRates : Page
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MuhasebeStokDB"].ConnectionString;

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                await LoadExchangeRates();
                LoadHistoricalExchangeRates();
                MarkCalendarDates();
            }
        }

        private async Task LoadExchangeRates()
        {
            if (!IsDataAlreadyLoadedForToday())
            {
                CurrencyService currencyService = new CurrencyService();
                List<ExchangeRate> exchangeRates = await currencyService.GetExchangeRatesAsync();

                ExchangeRatesService exchangeRatesService = new ExchangeRatesService(_connectionString);
                await exchangeRatesService.SaveExchangeRatesAsync(exchangeRates);
            }

            LoadExchangeRatesFromDatabase();
        }

        private bool IsDataAlreadyLoadedForToday()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM DovizKurlari WHERE CAST(KurTarihi AS DATE) = CAST(GETDATE() AS DATE)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Today", DateTime.UtcNow.Date);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private void LoadExchangeRatesFromDatabase()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        Dovizler.DovizKodu AS CurrencyCode, 
                        Dovizler.DovizAdi AS CurrencyName, 
                        DovizKurlari.AlisKur AS ForexBuying, 
                        DovizKurlari.SatisKur AS ForexSelling, 
                        DovizKurlari.KurTarihi AS Date,
                        CASE 
                            WHEN Dovizler.DovizKodu = 'TRY' THEN 'TL'
                            WHEN Dovizler.DovizKodu = 'UZS' THEN 'som'
                            ELSE '' 
                        END AS Unit
                    FROM DovizKurlari 
                    INNER JOIN Dovizler ON DovizKurlari.DovizID = Dovizler.DovizID 
                    WHERE CAST(DovizKurlari.KurTarihi AS DATE) = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                gvExchangeRates.DataSource = dt;
                gvExchangeRates.DataBind();
            }
        }

        private void LoadHistoricalExchangeRates(DateTime? filterDate = null, string sortExpression = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        Dovizler.DovizKodu AS CurrencyCode, 
                        Dovizler.DovizAdi AS CurrencyName, 
                        DovizKurlari.AlisKur AS ForexBuying, 
                        DovizKurlari.SatisKur AS ForexSelling, 
                        DovizKurlari.KurTarihi AS Date,
                        CASE 
                            WHEN Dovizler.DovizKodu = 'TRY' THEN 'TL'
                            WHEN Dovizler.DovizKodu = 'UZS' THEN 'som'
                            ELSE '' 
                        END AS Unit
                    FROM DovizKurlari 
                    INNER JOIN Dovizler ON DovizKurlari.DovizID = Dovizler.DovizID 
                    WHERE DovizKurlari.SoftDelete = 0";

                if (filterDate.HasValue && filterDate.Value != DateTime.MinValue)
                {
                    query += " AND DovizKurlari.KurTarihi = @FilterDate";
                }

                if (!string.IsNullOrEmpty(sortExpression))
                {
                    query += $" ORDER BY {sortExpression}";
                }
                else
                {
                    query += " ORDER BY DovizKurlari.KurTarihi DESC";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                if (filterDate.HasValue && filterDate.Value != DateTime.MinValue)
                {
                    cmd.Parameters.AddWithValue("@FilterDate", filterDate.Value);
                }

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                gvHistoricalExchangeRates.DataSource = dt;
                gvHistoricalExchangeRates.DataBind();
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime filterDate = calFilterDate.SelectedDate;
            if (filterDate != DateTime.MinValue)
            {
                LoadHistoricalExchangeRates(filterDate);
            }
            else
            {
                LoadHistoricalExchangeRates();
            }
        }

        protected void gvHistoricalExchangeRates_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHistoricalExchangeRates.PageIndex = e.NewPageIndex;
            DateTime filterDate = calFilterDate.SelectedDate;
            if (filterDate != DateTime.MinValue)
            {
                LoadHistoricalExchangeRates(filterDate);
            }
            else
            {
                LoadHistoricalExchangeRates();
            }
        }

        protected void gvHistoricalExchangeRates_Sorting(object sender, GridViewSortEventArgs e)
        {
            DateTime filterDate = calFilterDate.SelectedDate;
            if (filterDate != DateTime.MinValue)
            {
                LoadHistoricalExchangeRates(filterDate, e.SortExpression);
            }
            else
            {
                LoadHistoricalExchangeRates(null, e.SortExpression);
            }
        }

        protected void calFilterDate_DayRender(object sender, DayRenderEventArgs e)
        {
            if (e.Day.IsOtherMonth)
            {
                e.Cell.Text = string.Empty;
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM DovizKurlari WHERE KurTarihi = @Date";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Date", e.Day.Date);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();

                    if (count > 0)
                    {
                        e.Cell.BackColor = System.Drawing.Color.LightGreen;
                    }
                }
            }
        }

        private void MarkCalendarDates()
        {
            calFilterDate.VisibleDate = DateTime.Today;
        }
    }
}
