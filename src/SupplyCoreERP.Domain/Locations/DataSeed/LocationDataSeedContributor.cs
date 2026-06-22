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

namespace SupplyCoreERP.Locations.DataSeed;

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
        Continent asia = await CreateContinentAsync("Asia");
        Continent africa = await CreateContinentAsync("Africa");
        Continent europe = await CreateContinentAsync("Europe");
        Continent northAmerica = await CreateContinentAsync("North America");
        Continent southAmerica = await CreateContinentAsync("South America");
        Continent oceania = await CreateContinentAsync("Oceania");

        // SEED COUNTRIES 
        #region Asia
        Country vietnam = await CreateCountryAsync(asia.Id, "VNM", "VietNam");
        Country thailand = await CreateCountryAsync(asia.Id, "THA", "Thailand");
        Country malaysia = await CreateCountryAsync(asia.Id, "MYS", "Malaysia");
        Country japan = await CreateCountryAsync(asia.Id, "JPN", "Japan");
        Country china = await CreateCountryAsync(asia.Id, "CHN", "China");
        Country korea = await CreateCountryAsync(asia.Id, "KOR", "Korea");
        Country singapore = await CreateCountryAsync(asia.Id, "SGP", "Singapore");
        Country philippines = await CreateCountryAsync(asia.Id, "PHL", "Philippines");
        Country india = await CreateCountryAsync(asia.Id, "IND", "India");
        Country dubai = await CreateCountryAsync(asia.Id, "ARE", "United Arab Emirates");
        Country indonesia = await CreateCountryAsync(asia.Id, "IDN", "Indonesia");
        #endregion

        #region Europe
        Country uk = await CreateCountryAsync(europe.Id, "GBR", "United Kingdom");
        Country france = await CreateCountryAsync(europe.Id, "FRA", "France");
        Country germany = await CreateCountryAsync(europe.Id, "DEU", "Germany");
        Country italy = await CreateCountryAsync(europe.Id, "ITA", "Italy");
        Country spain = await CreateCountryAsync(europe.Id, "ESP", "Spain");
        Country netherlands = await CreateCountryAsync(europe.Id, "NLD", "Netherlands");
        Country belgium = await CreateCountryAsync(europe.Id, "BEL", "Belgium");
        Country switzerland = await CreateCountryAsync(europe.Id, "CHE", "Switzerland");
        Country ireland = await CreateCountryAsync(europe.Id, "IRL", "Ireland");
        #endregion

        #region North America
        Country usa = await CreateCountryAsync(northAmerica.Id, "USA", "United States");
        Country canada = await CreateCountryAsync(northAmerica.Id, "CAN", "Canada");
        Country mexico = await CreateCountryAsync(northAmerica.Id, "MEX", "Mexico");
        Country costaRica = await CreateCountryAsync(northAmerica.Id, "CRI", "Costa Rica");
        #endregion

        #region South America
        Country brazil = await CreateCountryAsync(southAmerica.Id, "BRA", "Brazil");
        Country argentina = await CreateCountryAsync(southAmerica.Id, "ARG", "Argentina");
        Country colombia = await CreateCountryAsync(southAmerica.Id, "COL", "Colombia");
        Country chile = await CreateCountryAsync(southAmerica.Id, "CHL", "Chile");
        Country peru = await CreateCountryAsync(southAmerica.Id, "PER", "Peru");
        Country venezuela = await CreateCountryAsync(southAmerica.Id, "VEN", "Venezuela");
        #endregion

        #region Africa
        Country nigeria = await CreateCountryAsync(africa.Id, "NGA", "Nigeria");
        Country egypt = await CreateCountryAsync(africa.Id, "EGY", "Egypt");
        Country morocco = await CreateCountryAsync(africa.Id, "MAR", "Morocco");
        Country southAfrica = await CreateCountryAsync(africa.Id, "ZAF", "South Africa");
        #endregion

        #region Oceania
        Country australia = await CreateCountryAsync(oceania.Id, "AUS", "Australia");
        Country newZealand = await CreateCountryAsync(oceania.Id, "NZL", "New Zealand");
        #endregion



        // 3. SEED CITIES 
        #region 34 CITIES IN VIETNAM
        City hcm = await CreateCityAsync(vietnam.Id, "Tp. Hồ Chí Minh");
        City hanoi = await CreateCityAsync(vietnam.Id, "Hà Nội");
        City danang = await CreateCityAsync(vietnam.Id, "Tp. Đà Nẵng");
        City haiphong = await CreateCityAsync(vietnam.Id, "Tp. Hải Phòng");
        City cantho = await CreateCityAsync(vietnam.Id, "Tp. Cần Thơ");
        City tuyenquang = await CreateCityAsync(vietnam.Id, "Tuyên Quang");
        City laocai = await CreateCityAsync(vietnam.Id, "Lào Cai");
        City thainguyen = await CreateCityAsync(vietnam.Id, "Thái Nguyên");
        City phutho = await CreateCityAsync(vietnam.Id, "Phú Thọ");
        City bacninh = await CreateCityAsync(vietnam.Id, "Bắc Ninh");
        City hungyen = await CreateCityAsync(vietnam.Id, "Hưng Yên");
        City ninhbinh = await CreateCityAsync(vietnam.Id, "Ninh Bình");
        City quangtri = await CreateCityAsync(vietnam.Id, "Quảng Trị");
        City quangngai = await CreateCityAsync(vietnam.Id, "Quảng Ngãi");
        City gialai = await CreateCityAsync(vietnam.Id, "Gia Lai");
        City khanhhoa = await CreateCityAsync(vietnam.Id, "Khánh Hòa");
        City lamdong = await CreateCityAsync(vietnam.Id, "Lâm Đồng");
        City daklak = await CreateCityAsync(vietnam.Id, "Đắk Lắk");
        City dongnai = await CreateCityAsync(vietnam.Id, "Đồng Nai");
        City tayninh = await CreateCityAsync(vietnam.Id, "Tây Ninh");
        City vinhlong = await CreateCityAsync(vietnam.Id, "Vĩnh Long");
        City dongthap = await CreateCityAsync(vietnam.Id, "Đồng Tháp");
        City camau = await CreateCityAsync(vietnam.Id, "Cà Mau");
        City angiang = await CreateCityAsync(vietnam.Id, "An Giang");
        #endregion

        #region CITIES IN THAILAND
        City bangkok = await CreateCityAsync(thailand.Id, "Bangkok");
        City chiangmai = await CreateCityAsync(thailand.Id, "Chiang Mai");
        City chiangrai = await CreateCityAsync(thailand.Id, "Chiang Rai");
        City phuket = await CreateCityAsync(thailand.Id, "Phuket");
        City chonburi = await CreateCityAsync(thailand.Id, "Chonburi");
        City ayutthaya = await CreateCityAsync(thailand.Id, "Ayutthaya");
        City nakhonratchasima = await CreateCityAsync(thailand.Id, "Nakhon Ratchasima");
        #endregion

        #region CITIES IN MALAYSIA
        City kualalumpur = await CreateCityAsync(malaysia.Id, "Kuala Lumpur");
        City penang = await CreateCityAsync(malaysia.Id, "Penang");
        City selangor = await CreateCityAsync(malaysia.Id, "Selangor");
        City johor = await CreateCityAsync(malaysia.Id, "Johor");
        City sabah = await CreateCityAsync(malaysia.Id, "Sabah");
        City sarawak = await CreateCityAsync(malaysia.Id, "Sarawak");
        #endregion

        #region CITIES IN SINGAPORE
        City central = await CreateCityAsync(singapore.Id, "Central Singapore");
        City north = await CreateCityAsync(singapore.Id, "North Singapore");
        City south = await CreateCityAsync(singapore.Id, "South Singapore");
        City east = await CreateCityAsync(singapore.Id, "East Singapore");
        City west = await CreateCityAsync(singapore.Id, "West Singapore");
        #endregion

        #region CiTIES IN JAPAN
        City tokyo = await CreateCityAsync(japan.Id, "Tokyo");
        City osaka = await CreateCityAsync(japan.Id, "Osaka");
        City kyoto = await CreateCityAsync(japan.Id, "Kyoto");
        City hokkaido = await CreateCityAsync(japan.Id, "Hokkaido");
        City aichi = await CreateCityAsync(japan.Id, "Aichi");
        City fukuoka = await CreateCityAsync(japan.Id, "Fukuoka");
        City hiroshima = await CreateCityAsync(japan.Id, "Hiroshima");
        City miyagi = await CreateCityAsync(japan.Id, "Miyagi");
        City okinawa = await CreateCityAsync(japan.Id, "Okinawa");
        City saitama = await CreateCityAsync(japan.Id, "Saitama");
        City chiba = await CreateCityAsync(japan.Id, "Chiba");
        City kanagawa = await CreateCityAsync(japan.Id, "Kanagawa");
        #endregion

        #region CITIES IN KOREA
        City seoul = await CreateCityAsync(korea.Id, "Seoul");
        City busan = await CreateCityAsync(korea.Id, "Busan");
        City incheon = await CreateCityAsync(korea.Id, "Incheon");
        City daegu = await CreateCityAsync(korea.Id, "Daegu");
        City daejeon = await CreateCityAsync(korea.Id, "Daejeon");
        City gwangju = await CreateCityAsync(korea.Id, "Gwangju");
        City ulsan = await CreateCityAsync(korea.Id, "Ulsan");
        City gyeonggi = await CreateCityAsync(korea.Id, "Gyeonggi");
        City gangwon = await CreateCityAsync(korea.Id, "Gangwon");
        City jeju = await CreateCityAsync(korea.Id, "Jeju");
        #endregion

        #region CITIES IN USA
        City newYork = await CreateCityAsync(usa.Id, "New York");
        City losAngeles = await CreateCityAsync(usa.Id, "Los Angeles");
        City chicago = await CreateCityAsync(usa.Id, "Chicago");
        City houston = await CreateCityAsync(usa.Id, "Houston");
        City phoenix = await CreateCityAsync(usa.Id, "Phoenix");
        City california = await CreateCityAsync(usa.Id, "California");
        City florida = await CreateCityAsync(usa.Id, "Florida");
        City texas = await CreateCityAsync(usa.Id, "Texas");
        City washingtonDC = await CreateCityAsync(usa.Id, "Washington D.C.");
        City boston = await CreateCityAsync(usa.Id, "Boston");
        City atlanta = await CreateCityAsync(usa.Id, "Atlanta");
        City miami = await CreateCityAsync(usa.Id, "Miami");
        City seattle = await CreateCityAsync(usa.Id, "Seattle");
        City denver = await CreateCityAsync(usa.Id, "Denver");
        City sanFrancisco = await CreateCityAsync(usa.Id, "San Francisco");
        City lasVegas = await CreateCityAsync(usa.Id, "Las Vegas");
        #endregion

        #region CITIES IN UK
        City london = await CreateCityAsync(uk.Id, "London");
        City manchester = await CreateCityAsync(uk.Id, "Manchester");
        City birmingham = await CreateCityAsync(uk.Id, "Birmingham");
        City leeds = await CreateCityAsync(uk.Id, "Leeds");
        City glasgow = await CreateCityAsync(uk.Id, "Glasgow");
        City sheffield = await CreateCityAsync(uk.Id, "Sheffield");
        City liverpool = await CreateCityAsync(uk.Id, "Liverpool");
        City bristol = await CreateCityAsync(uk.Id, "Bristol");
        City newcastle = await CreateCityAsync(uk.Id, "Newcastle");
        City nottingham = await CreateCityAsync(uk.Id, "Nottingham");
        City southampton = await CreateCityAsync(uk.Id, "Southampton");
        City brighton = await CreateCityAsync(uk.Id, "Brighton");
        City leicester = await CreateCityAsync(uk.Id, "Leicester");
        City coventry = await CreateCityAsync(uk.Id, "Coventry");
        City cardiff = await CreateCityAsync(uk.Id, "Cardiff");
        City belfast = await CreateCityAsync(uk.Id, "Belfast");
        City portsmouth = await CreateCityAsync(uk.Id, "Portsmouth");
        City plymouth = await CreateCityAsync(uk.Id, "Plymouth");
        City derby = await CreateCityAsync(uk.Id, "Derby");
        City swansea = await CreateCityAsync(uk.Id, "Swansea");
        City reading = await CreateCityAsync(uk.Id, "Reading");
        City norwich = await CreateCityAsync(uk.Id, "Norwich");
        City hull = await CreateCityAsync(uk.Id, "Hull");
        City oxford = await CreateCityAsync(uk.Id, "Oxford");
        City cambridge = await CreateCityAsync(uk.Id, "Cambridge");
        City york = await CreateCityAsync(uk.Id, "York");
        City blackpool = await CreateCityAsync(uk.Id, "Blackpool");
        City southend = await CreateCityAsync(uk.Id, "Southend-on-Sea");
        City bath = await CreateCityAsync(uk.Id, "Bath");
        City winchester = await CreateCityAsync(uk.Id, "Winchester");
        #endregion

        #region CITIES IN SWITZERLAND
        City zurich = await CreateCityAsync(switzerland.Id, "Zurich");
        City geneva = await CreateCityAsync(switzerland.Id, "Geneva");
        City basel = await CreateCityAsync(switzerland.Id, "Basel");
        City lausanne = await CreateCityAsync(switzerland.Id, "Lausanne");
        City bern = await CreateCityAsync(switzerland.Id, "Bern");
        City winterthur = await CreateCityAsync(switzerland.Id, "Winterthur");
        City stGallen = await CreateCityAsync(switzerland.Id, "St. Gallen");
        City lugano = await CreateCityAsync(switzerland.Id, "Lugano");
        City lucerne = await CreateCityAsync(switzerland.Id, "Lucerne");
        City thun = await CreateCityAsync(switzerland.Id, "Thun");
        City schaffhausen = await CreateCityAsync(switzerland.Id, "Schaffhausen");
        City fribourg = await CreateCityAsync(switzerland.Id, "Fribourg");
        City solothurn = await CreateCityAsync(switzerland.Id, "Solothurn");
        City neuchatel = await CreateCityAsync(switzerland.Id, "Neuchatel");
        City bellinzona = await CreateCityAsync(switzerland.Id, "Bellinzona");
        City sion = await CreateCityAsync(switzerland.Id, "Sion");
        City chur = await CreateCityAsync(switzerland.Id, "Chur");
        City glarus = await CreateCityAsync(switzerland.Id, "Glarus");
        City appenzell = await CreateCityAsync(switzerland.Id, "Appenzell");
        City altdorf = await CreateCityAsync(switzerland.Id, "Altdorf");
        City andermatt = await CreateCityAsync(switzerland.Id, "Andermatt");
        City davos = await CreateCityAsync(switzerland.Id, "Davos");
        City engelberg = await CreateCityAsync(switzerland.Id, "Engelberg");
        City flims = await CreateCityAsync(switzerland.Id, "Flims");
        #endregion

        #region CITIES IN CHINA
        City beijing = await CreateCityAsync(china.Id, "Beijing");
        City shanghai = await CreateCityAsync(china.Id, "Shanghai");
        City guangzhou = await CreateCityAsync(china.Id, "Guangzhou");
        City shenzhen = await CreateCityAsync(china.Id, "Shenzhen");
        City chengdu = await CreateCityAsync(china.Id, "Chengdu");
        City hangzhou = await CreateCityAsync(china.Id, "Hangzhou");
        City wuhan = await CreateCityAsync(china.Id, "Wuhan");
        City xiAn = await CreateCityAsync(china.Id, "Xi'an");
        City nanning = await CreateCityAsync(china.Id, "Nanning");
        City changsha = await CreateCityAsync(china.Id, "Changsha");
        City kunming = await CreateCityAsync(china.Id, "Kunming");
        City fuzhou = await CreateCityAsync(china.Id, "Fuzhou");
        City shenyang = await CreateCityAsync(china.Id, "Shenyang");
        City harbin = await CreateCityAsync(china.Id, "Harbin");
        City qingdao = await CreateCityAsync(china.Id, "Qingdao");
        City dalian = await CreateCityAsync(china.Id, "Dalian");
        City jinan = await CreateCityAsync(china.Id, "Jinan");
        City zhengzhou = await CreateCityAsync(china.Id, "Zhengzhou");
        City xiamen = await CreateCityAsync(china.Id, "Xiamen");
        City nanchang = await CreateCityAsync(china.Id, "Nanchang");
        City taiyuan = await CreateCityAsync(china.Id, "Taiyuan");
        City changchun = await CreateCityAsync(china.Id, "Changchun");
        City fuzhouChina = await CreateCityAsync(china.Id, "Fuzhou");
        City nanningChina = await CreateCityAsync(china.Id, "Nanning");
        City shijiazhuang = await CreateCityAsync(china.Id, "Shijiazhuang");
        City wenzhou = await CreateCityAsync(china.Id, "Wenzhou");
        City zhongshan = await CreateCityAsync(china.Id, "Zhongshan");
        City suzhou = await CreateCityAsync(china.Id, "Suzhou");
        City nanjing = await CreateCityAsync(china.Id, "Nanjing");
        City hefei = await CreateCityAsync(china.Id, "Hefei");
        City xuzhou = await CreateCityAsync(china.Id, "Xuzhou");
        City yantai = await CreateCityAsync(china.Id, "Yantai");
        City zibo = await CreateCityAsync(china.Id, "Zibo");
        City luoyang = await CreateCityAsync(china.Id, "Luoyang");
        City baoding = await CreateCityAsync(china.Id, "Baoding");
        City zhangzhou = await CreateCityAsync(china.Id, "Zhangzhou");
        City yancheng = await CreateCityAsync(china.Id, "Yancheng");
        City jinhua = await CreateCityAsync(china.Id, "Jinhua");
        City taizhou = await CreateCityAsync(china.Id, "Taizhou");
        City zhangjiagang = await CreateCityAsync(china.Id, "Zhangjiagang");
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
        Continent? existing = await _continentRepository.FirstOrDefaultAsync(x => x.Name == name);
        if (existing != null)
        {
            return existing;
        }

        return await _continentRepository.InsertAsync(
            new Continent(_guidGenerator.Create(), name),
            autoSave: true
        );
    }

    private async Task<Country> CreateCountryAsync(Guid continentId, string iso, string name)
    {
        Country? existing = await _countryRepository.FirstOrDefaultAsync(x => x.ISO == iso);
        if (existing != null)
        {
            return existing;
        }

        return await _countryRepository.InsertAsync(
            new Country(_guidGenerator.Create(), continentId, iso, name),
            autoSave: true
        );
    }

    private async Task<City> CreateCityAsync(Guid countryId, string name)
    {
        City? existing = await _cityRepository.FirstOrDefaultAsync(x => x.Name == name && x.CountryId == countryId);
        if (existing != null)
        {
            return existing;
        }

        return await _cityRepository.InsertAsync(
            new City(_guidGenerator.Create(), countryId, name),
            autoSave: true
        );
    }

    private async Task CreateAreaAsync(Guid cityId, string zipCode, string name)
    {
        Area? existing = await _areaRepository.FirstOrDefaultAsync(x => x.Name == name && x.CityId == cityId);
        if (existing == null)
        {
            await _areaRepository.InsertAsync(
                new Area(_guidGenerator.Create(), cityId, zipCode, name),
                autoSave: true
            );
        }
    }
}






