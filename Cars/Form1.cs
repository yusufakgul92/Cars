using Cars.Helper;
using Cars.Model;
using CefSharp;
using CefSharp.OffScreen;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Cars
{
    public partial class Form1 : Form
    {
        private ChromiumWebBrowser brw;
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_Click(object sender, EventArgs e)
        {

            const string url = "https://www.cars.com/signin/?redirect_path=%2F";

            AsyncContext.Run(async delegate
            {
                var settings = new CefSettings()
                {
                    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
                };

                var success = await Cef.InitializeAsync(settings, performDependencyCheck: true, browserProcessHandler: null);

                if (!success)
                {
                    throw new Exception("Unable to initialize CEF, check the log file.");
                }

                using (var browser = new ChromiumWebBrowser(url))
                {
                    var initialLoadResponse = await browser.WaitForInitialLoadAsync();

                    if (!initialLoadResponse.Success)
                    {
                        throw new Exception(string.Format("Page load failed with ErrorCode:{0}, HttpStatusCode:{1}", initialLoadResponse.ErrorCode, initialLoadResponse.HttpStatusCode));
                    }

                    #region Login
                    browser.ExecuteScriptAsync($@"document.getElementById('email').value = 'johngerson808@gmail.com'");
                    browser.ExecuteScriptAsync($@"document.getElementById('password').value = 'test8008'");
                    browser.ExecuteScriptAsync("document.getElementsByClassName('sds-button')[0].click()");
                    #endregion

                    await Task.Delay(500);

                    #region Setting Up
                    browser.ExecuteScriptAsync($@"document.querySelector('#make-model-search-stocktype').value = 'used'");
                    browser.ExecuteScriptAsync($@"document.querySelector('#makes').value = 'tesla'");
                    browser.ExecuteScriptAsync($@"document.querySelector('#make-model-max-price').value = '100000'");
                    browser.ExecuteScriptAsync($@"document.querySelector('#make-model-maximum-distance').value = 'all'");
                    browser.ExecuteScriptAsync($@"document.getElementById('make-model-zip').value = '94596'");
                    browser.ExecuteScriptAsync($@"document.getElementsByClassName('sds-button')[0].click()");
                    #endregion Setting Up

                    await Task.Delay(500);

                    List<Car> carList = new List<Car>();
                    List<CarDetail> carDetails = new List<CarDetail>();

                    #region Car Models
                    string[] carModels = { "tesla-model_x", "tesla-model_3", "tesla-model_s", "tesla-model_y", "tesla-roadster" };
                    #endregion

                    foreach (string model in carModels)
                    {
                        #region Model Set Up
                        await browser.LoadUrlAsync("https://www.cars.com/");
                        await Task.Delay(500);
                        browser.ExecuteScriptAsync($@"document.querySelector('#models').value = '{model}'");
                        await Task.Delay(1000);
                        browser.ExecuteScriptAsync($@"document.getElementsByClassName('sds-button')[0].click()");
                        await Task.Delay(1000);

                        string html = await browser.GetSourceAsync();

                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(html);
                        await Task.Delay(500);

                        List<HtmlNode> cars = doc.DocumentNode.Descendants().Where(a => a.HasClass("vehicle-details"))?.ToList();

                        List<Car> fList = CreateCars(cars);

                        if (fList != null && fList.Count > 0)
                        {
                            carList.AddRange(fList);
                        }

                        browser.ExecuteScriptAsync($@"document.getElementById('pagination-direct-link-2').click()");
                        await Task.Delay(1000);
                        html = await browser.GetSourceAsync();
                        doc = new HtmlDocument();
                        doc.LoadHtml(html);
                        await Task.Delay(500);
                        List<HtmlNode> cars2 = doc.DocumentNode.Descendants().Where(a => a.Id.Contains("vehicle-card"))?.ToList();
                        List<Car> sList = CreateCars(cars);
                        if (sList != null && sList.Count > 0)
                        {
                            carList.AddRange(sList);
                        }

                        browser.ExecuteScriptAsync("document.getElementsByClassName('vehicle-card-link js-gallery-click-link')[0].click()");
                        await Task.Delay(5000);
                        html = await browser.GetSourceAsync();
                        doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        HtmlNode newUsed = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("new-used"));
                        HtmlNode listing = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("listing-title"));
                        HtmlNode mileage = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("listing-mileage"));
                        HtmlNode price = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("primary-price"));
                        HtmlNode sellerName = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("seller-name"));
                        HtmlNode priceDrop = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("secondary-price"));
                        HtmlNode descList = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("fancy-description-list"));
                        HtmlNode featList = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("vehicle-features-list"));
                        HtmlNode fanList = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("fancy-description-list"));
                        HtmlNode additionalFeatList = doc.DocumentNode.Descendants().FirstOrDefault(a => a.HasClass("auto-corrected-feature-list"));
                        var featureList = doc.DocumentNode.Descendants().Where(a => a.HasClass("vehicle-features-list"));

                        CarDetail carDetail = new CarDetail
                        {
                            Condition = newUsed?.InnerText?.Trim() ?? "",
                            Name = listing?.InnerText?.Trim() ?? "",
                            MileAge = mileage?.InnerText?.Trim() ?? "",
                            Price = price?.InnerText?.Trim() ?? "",
                            PriceDrop = priceDrop?.InnerText?.Trim() ?? "",
                            Features = featList?.ChildNodes?.Where(a => !string.IsNullOrWhiteSpace(a.InnerText)).Select(a => a.InnerText)?.ToList(),
                            AdditionalFeatures = additionalFeatList?.InnerText?.Split(',')?.ToList(),
                            Seller = sellerName?.InnerText?.Trim() ?? ""
                        };

                        for (int i = 0; i < featureList.Count(); i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    carDetail.Convenience = featureList.ElementAt(i).ChildNodes?.Where(a => !string.IsNullOrWhiteSpace(a.InnerText)).Select(a => a.InnerText)?.ToList();
                                    break;
                                case 1:
                                    carDetail.Entertainment = featureList.ElementAt(i).ChildNodes?.Where(a => !string.IsNullOrWhiteSpace(a.InnerText)).Select(a => a.InnerText)?.ToList();
                                    break;
                                case 2:
                                    carDetail.Exterior = featureList.ElementAt(i).ChildNodes?.Where(a => !string.IsNullOrWhiteSpace(a.InnerText)).Select(a => a.InnerText)?.ToList();
                                    break;
                                case 3:
                                    carDetail.Safety = featureList.ElementAt(i).ChildNodes?.Where(a => !string.IsNullOrWhiteSpace(a.InnerText)).Select(a => a.InnerText)?.ToList();
                                    break;
                                case 4:
                                    carDetail.Seating = featureList.ElementAt(i).ChildNodes?.Where(a => !string.IsNullOrWhiteSpace(a.InnerText)).Select(a => a.InnerText)?.ToList();
                                    break;
                                default:
                                    break;
                            }
                        }

                        carDetails.Add(carDetail);


                        #endregion  Model Set Up
                    }

                    string carDetailsFile = JsonConvert.SerializeObject(carDetails);
                    string carsFile = JsonConvert.SerializeObject(carList);
                    string carsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cars.json");
                    string carDetailsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "cardetails.json");

                    File.WriteAllText(carDetailsPath, carDetailsFile);
                    File.WriteAllText(carsPath, carsFile);

                }

                Cef.Shutdown();
            });

        }
        private static List<Car> CreateCars(List<HtmlNode> cars)
        {
            List<Car> carList = new List<Car>();
            foreach (HtmlNode item in cars)
            {
                var aa = item.ChildNodes.SelectMany(a => a.GetClasses()).ToList();
                HtmlNode title = item.ChildNodes.FirstOrDefault(a => a.HasClass("vehicle-card-link"))?.ChildNodes.FirstOrDefault(a => a.HasClass("title"));
                HtmlNode mileAge = item.ChildNodes.FirstOrDefault(a => a.HasClass("mileage"));
                HtmlNode estimate = item.ChildNodes?.FirstOrDefault(a => a.HasClass("estimated-monthly-payments-tooltip"))?.
                    ChildNodes.FirstOrDefault(a => a.HasClass("list-item-tooltip-container"))?.
                    ChildNodes.FirstOrDefault(a => a.HasClass("sds-tooltip"))?.
                    ChildNodes.FirstOrDefault(a => a.HasClass("sds-link"))?.
                    ChildNodes.FirstOrDefault(a => a.HasClass("js-estimated-monthly-payment-formatted-value-with-abr"));
                HtmlNode price = item.ChildNodes.FirstOrDefault(a => a.HasClass("price-section-vehicle-card")).ChildNodes.FirstOrDefault(a => a.HasClass("primary-price"));
                HtmlNode priceDrop = item.ChildNodes.FirstOrDefault(a => a.HasClass("price-section-vehicle-card")).ChildNodes.FirstOrDefault(a => a.HasClass("secondary-price"));
                HtmlNode rating = item.ChildNodes.FirstOrDefault(a => a.HasClass("vehicle-dealer"))?.
                    ChildNodes?.FirstOrDefault(a => a.HasClass("sds-rating"))?.
                    ChildNodes?.FirstOrDefault(a => a.HasClass("sds-rating__count"));
                HtmlNode dealer = item.ChildNodes?.FirstOrDefault(a => a.HasClass("vehicle-dealer"))?.ChildNodes?.FirstOrDefault(a => a.HasClass("dealer-name"));
                HtmlNode condition = item.ChildNodes.FirstOrDefault(a => a.HasClass("stock-type"));


                Car car = new Car
                {
                    Name = title?.InnerText?.Trim() ?? "",
                    MileAge = mileAge?.InnerText?.Trim() ?? "",
                    EstMonth = estimate?.InnerHtml?.Trim() ?? "",
                    Price = price?.InnerText?.Trim() ?? "",
                    PriceDrop = priceDrop?.InnerText?.Trim() ?? "",
                    Rating = rating?.InnerText?.Trim() ?? "",
                    Condition = condition?.InnerText?.Trim() ?? "",
                    Seller = dealer?.InnerText?.Replace("<strong>", "")?.Replace("</strong>", "")?.Trim() ?? ""
                };

                carList.Add(car);
            }
            return carList;
        }

    }
}
