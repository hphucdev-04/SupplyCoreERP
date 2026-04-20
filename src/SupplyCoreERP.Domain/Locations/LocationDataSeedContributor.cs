using System;
using System.Threading.Tasks;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Locations
{
    public class LocationDataSeedContributor : IDataSeedContributor, ITransientDependency
    {
        private readonly IRepository<Continent, Guid> _continentRepository;
        private readonly IRepository<Country, Guid> _countryRepository;
        private readonly IRepository<City, Guid> _cityRepository;
        private readonly IRepository<Area, Guid> _areaRepository;
        private readonly IGuidGenerator _guidGenerator;

        public LocationDataSeedContributor(
            IRepository<Continent, Guid> continentRepository,
            IRepository<Country, Guid> countryRepository,
            IRepository<City, Guid> cityRepository,
            IRepository<Area, Guid> areaRepository,
            IGuidGenerator guidGenerator)
        {
            _continentRepository = continentRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _areaRepository = areaRepository;
            _guidGenerator = guidGenerator;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            // SEED CONTINENTS
            var asia = await CreateContinentAsync("Asia");
            var africa = await CreateContinentAsync("Africa");
            var europe = await CreateContinentAsync("Europe");
            var northAmerica = await CreateContinentAsync("North America");
            var southAmerica = await CreateContinentAsync("South America");
            var oceania = await CreateContinentAsync("Oceania");

            // SEED COUNTRIES 
            #region Asia
            var vietnam = await CreateCountryAsync(asia.Id, "VNM", "Viet Nam");
            var thailand = await CreateCountryAsync(asia.Id, "THA", "Thailand");
            var malaysia = await CreateCountryAsync(asia.Id, "MYS", "Malaysia");
            var japan = await CreateCountryAsync(asia.Id, "JPN", "Japan");
            var china = await CreateCountryAsync(asia.Id, "CHN", "China");
            var korea = await CreateCountryAsync(asia.Id, "KOR", "Korea");
            var singapore = await CreateCountryAsync(asia.Id, "SGP", "Singapore");
            var philippines = await CreateCountryAsync(asia.Id, "PHL", "Philippines");
            var india = await CreateCountryAsync(asia.Id, "IND", "India");
            var dubai = await CreateCountryAsync(asia.Id, "ARE", "United Arab Emirates");
            var indonesia = await CreateCountryAsync(asia.Id, "IDN", "Indonesia");
            #endregion

            #region Europe
            var uk = await CreateCountryAsync(europe.Id, "GBR", "United Kingdom");
            var france = await CreateCountryAsync(europe.Id, "FRA", "France");
            var germany = await CreateCountryAsync(europe.Id, "DEU", "Germany");
            var italy = await CreateCountryAsync(europe.Id, "ITA", "Italy");
            var spain = await CreateCountryAsync(europe.Id, "ESP", "Spain");
            var netherlands = await CreateCountryAsync(europe.Id, "NLD", "Netherlands");
            var belgium = await CreateCountryAsync(europe.Id, "BEL", "Belgium");
            var switzerland = await CreateCountryAsync(europe.Id, "CHE", "Switzerland");
            #endregion

            #region North America
            var usa = await CreateCountryAsync(northAmerica.Id, "USA", "United States");
            var canada = await CreateCountryAsync(northAmerica.Id, "CAN", "Canada");
            var mexico = await CreateCountryAsync(northAmerica.Id, "MEX", "Mexico");
            var costaRica = await CreateCountryAsync(northAmerica.Id, "CRI", "Costa Rica");
            #endregion

            #region South America
            var brazil = await CreateCountryAsync(southAmerica.Id, "BRA", "Brazil");
            var argentina = await CreateCountryAsync(southAmerica.Id, "ARG", "Argentina");
            var colombia = await CreateCountryAsync(southAmerica.Id, "COL", "Colombia");
            var chile = await CreateCountryAsync(southAmerica.Id, "CHL", "Chile");
            var peru = await CreateCountryAsync(southAmerica.Id, "PER", "Peru");
            var venezuela = await CreateCountryAsync(southAmerica.Id, "VEN", "Venezuela");
            #endregion

            #region Africa
            var nigeria = await CreateCountryAsync(africa.Id, "NGA", "Nigeria");
            var egypt = await CreateCountryAsync(africa.Id, "EGY", "Egypt");
            var morocco = await CreateCountryAsync(africa.Id, "MAR", "Morocco");
            var southAfrica = await CreateCountryAsync(africa.Id, "ZAF", "South Africa");
            #endregion

            #region Oceania
            var australia = await CreateCountryAsync(oceania.Id, "AUS", "Australia");
            var newZealand = await CreateCountryAsync(oceania.Id, "NZL", "New Zealand");
            #endregion



            // 3. SEED CITIES 
            #region 34 CITIES IN VIETNAM
            var hcm = await CreateCityAsync(vietnam.Id, "Tp. Hồ Chí Minh");
            var hanoi = await CreateCityAsync(vietnam.Id, "Hà Nội");
            var danang = await CreateCityAsync(vietnam.Id, "Tp. Đà Nẵng");
            var haiphong = await CreateCityAsync(vietnam.Id, "Tp. Hải Phòng");
            var cantho = await CreateCityAsync(vietnam.Id, "Tp. Cần Thơ");
            var tuyenquang = await CreateCityAsync(vietnam.Id, "Tuyên Quang");
            var laocai = await CreateCityAsync(vietnam.Id, "Lào Cai");
            var thainguyen = await CreateCityAsync(vietnam.Id, "Thái Nguyên");
            var phutho = await CreateCityAsync(vietnam.Id, "Phú Thọ");
            var bacninh = await CreateCityAsync(vietnam.Id, "Bắc Ninh");
            var hungyen = await CreateCityAsync(vietnam.Id, "Hưng Yên");
            var ninhbinh = await CreateCityAsync(vietnam.Id, "Ninh Bình");
            var quangtri = await CreateCityAsync(vietnam.Id, "Quảng Trị");
            var quangngai = await CreateCityAsync(vietnam.Id, "Quảng Ngãi");
            var gialai = await CreateCityAsync(vietnam.Id, "Gia Lai");
            var khanhhoa = await CreateCityAsync(vietnam.Id, "Khánh Hòa");
            var lamdong = await CreateCityAsync(vietnam.Id, "Lâm Đồng");
            var daklak = await CreateCityAsync(vietnam.Id, "Đắk Lắk");
            var dongnai = await CreateCityAsync(vietnam.Id, "Đồng Nai");
            var tayninh = await CreateCityAsync(vietnam.Id, "Tây Ninh");
            var vinhlong = await CreateCityAsync(vietnam.Id, "Vĩnh Long");
            var dongthap = await CreateCityAsync(vietnam.Id, "Đồng Tháp");
            var camau = await CreateCityAsync(vietnam.Id, "Cà Mau");
            var angiang = await CreateCityAsync(vietnam.Id, "An Giang");
            #endregion

            #region CITIES IN THAILAND
            var bangkok = await CreateCityAsync(thailand.Id, "Bangkok");
            var chiangmai = await CreateCityAsync(thailand.Id, "Chiang Mai");
            var chiangrai = await CreateCityAsync(thailand.Id, "Chiang Rai");
            var phuket = await CreateCityAsync(thailand.Id, "Phuket");
            var chonburi = await CreateCityAsync(thailand.Id, "Chonburi");
            var ayutthaya = await CreateCityAsync(thailand.Id, "Ayutthaya");
            var nakhonratchasima = await CreateCityAsync(thailand.Id, "Nakhon Ratchasima");
            #endregion

            #region CITIES IN MALAYSIA
            var kualalumpur = await CreateCityAsync(malaysia.Id, "Kuala Lumpur");
            var penang = await CreateCityAsync(malaysia.Id, "Penang");
            var selangor = await CreateCityAsync(malaysia.Id, "Selangor");
            var johor = await CreateCityAsync(malaysia.Id, "Johor");
            var sabah = await CreateCityAsync(malaysia.Id, "Sabah");
            var sarawak = await CreateCityAsync(malaysia.Id, "Sarawak");
            #endregion

            #region CITIES IN SINGAPORE
            var central = await CreateCityAsync(singapore.Id, "Central Singapore");
            var north = await CreateCityAsync(singapore.Id, "North Singapore");
            var south = await CreateCityAsync(singapore.Id, "South Singapore");
            var east = await CreateCityAsync(singapore.Id, "East Singapore");
            var west = await CreateCityAsync(singapore.Id, "West Singapore");
            #endregion

            #region CiTIES IN JAPAN
            var tokyo = await CreateCityAsync(japan.Id, "Tokyo");
            var osaka = await CreateCityAsync(japan.Id, "Osaka");
            var kyoto = await CreateCityAsync(japan.Id, "Kyoto");
            var hokkaido = await CreateCityAsync(japan.Id, "Hokkaido");
            var aichi = await CreateCityAsync(japan.Id, "Aichi");
            var fukuoka = await CreateCityAsync(japan.Id, "Fukuoka");
            var hiroshima = await CreateCityAsync(japan.Id, "Hiroshima");
            var miyagi = await CreateCityAsync(japan.Id, "Miyagi");
            var okinawa = await CreateCityAsync(japan.Id, "Okinawa");
            var saitama = await CreateCityAsync(japan.Id, "Saitama");
            var chiba = await CreateCityAsync(japan.Id, "Chiba");
            var kanagawa = await CreateCityAsync(japan.Id, "Kanagawa");
            #endregion

            #region CITIES IN KOREA
            var seoul = await CreateCityAsync(korea.Id, "Seoul");
            var busan = await CreateCityAsync(korea.Id, "Busan");
            var incheon = await CreateCityAsync(korea.Id, "Incheon");
            var daegu = await CreateCityAsync(korea.Id, "Daegu");
            var daejeon = await CreateCityAsync(korea.Id, "Daejeon");
            var gwangju = await CreateCityAsync(korea.Id, "Gwangju");
            var ulsan = await CreateCityAsync(korea.Id, "Ulsan");
            var gyeonggi = await CreateCityAsync(korea.Id, "Gyeonggi");
            var gangwon = await CreateCityAsync(korea.Id, "Gangwon");
            var jeju = await CreateCityAsync(korea.Id, "Jeju");
            #endregion

            #region CITIES IN USA
            var newYork = await CreateCityAsync(usa.Id, "New York");
            var losAngeles = await CreateCityAsync(usa.Id, "Los Angeles");
            var chicago = await CreateCityAsync(usa.Id, "Chicago");
            var houston = await CreateCityAsync(usa.Id, "Houston");
            var phoenix = await CreateCityAsync(usa.Id, "Phoenix");
            var california = await CreateCityAsync(usa.Id, "California");
            var florida = await CreateCityAsync(usa.Id, "Florida");
            var texas = await CreateCityAsync(usa.Id, "Texas");
            var washingtonDC = await CreateCityAsync(usa.Id, "Washington D.C.");
            var boston = await CreateCityAsync(usa.Id, "Boston");
            var atlanta = await CreateCityAsync(usa.Id, "Atlanta");
            var miami = await CreateCityAsync(usa.Id, "Miami");
            var seattle = await CreateCityAsync(usa.Id, "Seattle");
            var denver = await CreateCityAsync(usa.Id, "Denver");
            var sanFrancisco = await CreateCityAsync(usa.Id, "San Francisco");
            var lasVegas = await CreateCityAsync(usa.Id, "Las Vegas");
            #endregion

            #region CITIES IN UK
            var london = await CreateCityAsync(uk.Id, "London");
            var manchester = await CreateCityAsync(uk.Id, "Manchester");
            var birmingham = await CreateCityAsync(uk.Id, "Birmingham");
            var leeds = await CreateCityAsync(uk.Id, "Leeds");
            var glasgow = await CreateCityAsync(uk.Id, "Glasgow");
            var sheffield = await CreateCityAsync(uk.Id, "Sheffield");
            var liverpool = await CreateCityAsync(uk.Id, "Liverpool");
            var bristol = await CreateCityAsync(uk.Id, "Bristol");
            var newcastle = await CreateCityAsync(uk.Id, "Newcastle");
            var nottingham = await CreateCityAsync(uk.Id, "Nottingham");
            var southampton = await CreateCityAsync(uk.Id, "Southampton");
            var brighton = await CreateCityAsync(uk.Id, "Brighton");
            var leicester = await CreateCityAsync(uk.Id, "Leicester");
            var coventry = await CreateCityAsync(uk.Id, "Coventry");
            var cardiff = await CreateCityAsync(uk.Id, "Cardiff");
            var belfast = await CreateCityAsync(uk.Id, "Belfast");
            var portsmouth = await CreateCityAsync(uk.Id, "Portsmouth");
            var plymouth = await CreateCityAsync(uk.Id, "Plymouth");
            var derby = await CreateCityAsync(uk.Id, "Derby");
            var swansea = await CreateCityAsync(uk.Id, "Swansea");
            var reading = await CreateCityAsync(uk.Id, "Reading");
            var norwich = await CreateCityAsync(uk.Id, "Norwich");
            var hull = await CreateCityAsync(uk.Id, "Hull");
            var oxford = await CreateCityAsync(uk.Id, "Oxford");
            var cambridge = await CreateCityAsync(uk.Id, "Cambridge");
            var york = await CreateCityAsync(uk.Id, "York");
            var blackpool = await CreateCityAsync(uk.Id, "Blackpool");
            var southend = await CreateCityAsync(uk.Id, "Southend-on-Sea");
            var bath = await CreateCityAsync(uk.Id, "Bath");
            var winchester = await CreateCityAsync(uk.Id, "Winchester");
            #endregion

            #region CITIES IN SWITZERLAND
            var zurich = await CreateCityAsync(switzerland.Id, "Zurich");
            var geneva = await CreateCityAsync(switzerland.Id, "Geneva");
            var basel = await CreateCityAsync(switzerland.Id, "Basel");
            var lausanne = await CreateCityAsync(switzerland.Id, "Lausanne");
            var bern = await CreateCityAsync(switzerland.Id, "Bern");
            var winterthur = await CreateCityAsync(switzerland.Id, "Winterthur");
            var stGallen = await CreateCityAsync(switzerland.Id, "St. Gallen");
            var lugano = await CreateCityAsync(switzerland.Id, "Lugano");
            var lucerne = await CreateCityAsync(switzerland.Id, "Lucerne");
            var thun = await CreateCityAsync(switzerland.Id, "Thun");
            var schaffhausen = await CreateCityAsync(switzerland.Id, "Schaffhausen");
            var fribourg = await CreateCityAsync(switzerland.Id, "Fribourg");
            var solothurn = await CreateCityAsync(switzerland.Id, "Solothurn");
            var neuchatel = await CreateCityAsync(switzerland.Id, "Neuchâtel");
            var bellinzona = await CreateCityAsync(switzerland.Id, "Bellinzona");
            var sion = await CreateCityAsync(switzerland.Id, "Sion");
            var chur = await CreateCityAsync(switzerland.Id, "Chur");
            var glarus = await CreateCityAsync(switzerland.Id, "Glarus");
            var appenzell = await CreateCityAsync(switzerland.Id, "Appenzell");
            var altdorf = await CreateCityAsync(switzerland.Id, "Altdorf");
            var andermatt = await CreateCityAsync(switzerland.Id, "Andermatt");
            var davos = await CreateCityAsync(switzerland.Id, "Davos");
            var engelberg = await CreateCityAsync(switzerland.Id, "Engelberg");
            var flims = await CreateCityAsync(switzerland.Id, "Flims");
            #endregion

            #region CITIES IN CHINA
            var beijing = await CreateCityAsync(china.Id, "Beijing");
            var shanghai = await CreateCityAsync(china.Id, "Shanghai");
            var guangzhou = await CreateCityAsync(china.Id, "Guangzhou");
            var shenzhen = await CreateCityAsync(china.Id, "Shenzhen");
            var chengdu = await CreateCityAsync(china.Id, "Chengdu");
            var hangzhou = await CreateCityAsync(china.Id, "Hangzhou");
            var wuhan = await CreateCityAsync(china.Id, "Wuhan");
            var xiAn = await CreateCityAsync(china.Id, "Xi'an");
            var nanning = await CreateCityAsync(china.Id, "Nanning");
            var changsha = await CreateCityAsync(china.Id, "Changsha");
            var kunming = await CreateCityAsync(china.Id, "Kunming");
            var fuzhou = await CreateCityAsync(china.Id, "Fuzhou");
            var shenyang = await CreateCityAsync(china.Id, "Shenyang");
            var harbin = await CreateCityAsync(china.Id, "Harbin");
            var qingdao = await CreateCityAsync(china.Id, "Qingdao");
            var dalian = await CreateCityAsync(china.Id, "Dalian");
            var jinan = await CreateCityAsync(china.Id, "Jinan");
            var zhengzhou = await CreateCityAsync(china.Id, "Zhengzhou");
            var xiamen = await CreateCityAsync(china.Id, "Xiamen");
            var nanchang = await CreateCityAsync(china.Id, "Nanchang");
            var taiyuan = await CreateCityAsync(china.Id, "Taiyuan");
            var changchun = await CreateCityAsync(china.Id, "Changchun");
            var fuzhouChina = await CreateCityAsync(china.Id, "Fuzhou");
            var nanningChina = await CreateCityAsync(china.Id, "Nanning");
            var shijiazhuang = await CreateCityAsync(china.Id, "Shijiazhuang");
            var wenzhou = await CreateCityAsync(china.Id, "Wenzhou");
            var zhongshan = await CreateCityAsync(china.Id, "Zhongshan");
            var suzhou = await CreateCityAsync(china.Id, "Suzhou");
            var nanjing = await CreateCityAsync(china.Id, "Nanjing");
            var hefei = await CreateCityAsync(china.Id, "Hefei");
            var xuzhou = await CreateCityAsync(china.Id, "Xuzhou");
            var yantai = await CreateCityAsync(china.Id, "Yantai");
            var zibo = await CreateCityAsync(china.Id, "Zibo");
            var luoyang = await CreateCityAsync(china.Id, "Luoyang");
            var baoding = await CreateCityAsync(china.Id, "Baoding");
            var zhangzhou = await CreateCityAsync(china.Id, "Zhangzhou");
            var yancheng = await CreateCityAsync(china.Id, "Yancheng");
            var jinhua = await CreateCityAsync(china.Id, "Jinhua");
            var taizhou = await CreateCityAsync(china.Id, "Taizhou");
            var zhangjiagang = await CreateCityAsync(china.Id, "Zhangjiagang");
            #endregion

            // 4. SEED AREAS 
            #region AREAS IN HCM
            await CreateAreaAsync(hcm.Id, "70001", "Quận 1");
            await CreateAreaAsync(hcm.Id, "70003", "Quận 3");
            await CreateAreaAsync(hcm.Id, "70004", "Quận 4");
            await CreateAreaAsync(hcm.Id, "70005", "Quận 5");
            await CreateAreaAsync(hcm.Id, "70006", "Quận 6");
            await CreateAreaAsync(hcm.Id, "70007", "Quận 7");
            await CreateAreaAsync(hcm.Id, "70008", "Quận 8");
            await CreateAreaAsync(hcm.Id, "70010", "Quận 10");
            await CreateAreaAsync(hcm.Id, "70011", "Quận 11");
            await CreateAreaAsync(hcm.Id, "70012", "Quận 12");
            await CreateAreaAsync(hcm.Id, "70013", "Quận Bình Thạnh");
            await CreateAreaAsync(hcm.Id, "70014", "Quận Phú Nhuận");
            await CreateAreaAsync(hcm.Id, "70015", "Quận Gò Vấp");
            await CreateAreaAsync(hcm.Id, "70016", "Quận Tân Bình");
            await CreateAreaAsync(hcm.Id, "70017", "Quận Tân Phú");
            await CreateAreaAsync(hcm.Id, "70018", "Quận Bình Tân");
            await CreateAreaAsync(hcm.Id, "70019", "Huyện Bình Chánh");
            await CreateAreaAsync(hcm.Id, "70020", "Huyện Củ Chi");
            await CreateAreaAsync(hcm.Id, "70021", "Huyện Hóc Môn");
            await CreateAreaAsync(hcm.Id, "70022", "Huyện Nhà Bè");
            await CreateAreaAsync(hcm.Id, "70023", "Huyện Cần Giờ");
            await CreateAreaAsync(hcm.Id, "70024", "Tp. Thủ Đức");
            #endregion 
        }


        private async Task<Continent> CreateContinentAsync(string name)
        {
            var existing = await _continentRepository.FirstOrDefaultAsync(x => x.Name == name);
            if (existing != null) return existing;

            return await _continentRepository.InsertAsync(
                new Continent(_guidGenerator.Create(), name),
                autoSave: true
            );
        }

        private async Task<Country> CreateCountryAsync(Guid continentId, string iso, string name)
        {
            var existing = await _countryRepository.FirstOrDefaultAsync(x => x.ISO == iso);
            if (existing != null) return existing;

            return await _countryRepository.InsertAsync(
                new Country(_guidGenerator.Create(), continentId, iso, name),
                autoSave: true
            );
        }

        private async Task<City> CreateCityAsync(Guid countryId, string name)
        {
            var existing = await _cityRepository.FirstOrDefaultAsync(x => x.Name == name && x.CountryId == countryId);
            if (existing != null) return existing;

            return await _cityRepository.InsertAsync(
                new City(_guidGenerator.Create(), countryId, name),
                autoSave: true
            );
        }

        private async Task CreateAreaAsync(Guid cityId, string zipCode, string name)
        {
            var existing = await _areaRepository.FirstOrDefaultAsync(x => x.Name == name && x.CityId == cityId);
            if (existing == null)
            {
                await _areaRepository.InsertAsync(
                    new Area(_guidGenerator.Create(), cityId, zipCode, name),
                    autoSave: true
                );
            }
        }
    }
}