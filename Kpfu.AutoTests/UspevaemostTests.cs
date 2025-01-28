using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Kpfu.AutoTests;

[TestFixture]
public class UspevaemostTests
{
    private IWebDriver _driver;

    // todo: тут заменить на логин+пароль учетки студента

    #region Credentials

    private const string Login = "Login";
    private const string Pass = "Password";

    #endregion

    [SetUp]
    public void SetUp()
    {
        var options = new ChromeOptions();
        // options.Proxy = new Proxy { HttpProxy = "203.190.117.97:8076" };
        options.AddArgument("--start-maximized"); // задание размера окна
        if (!Debugger.IsAttached)
        {
            options.AddArgument("headless"); // запуск браузера в фоновом режиме
        }

        _driver = new ChromeDriver(options);

        _driver.Navigate().GoToUrl("https://kpfu.ru/");

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(drv => drv.FindElement(By.ClassName("lk-link")));
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }


    [TestCase(Login, Pass)]
    public async Task Login_Successful(string login, string pass)
    {
        var loginBtn = _driver.FindElement(By.ClassName("lk-link"));
        loginBtn.Click();

        var loginField = _driver.FindElement(By.XPath("//*[@id=\"eu_enter\"]/input[1]"));
        loginField.SendKeys(login);
        var passField = _driver.FindElement(By.XPath("//*[@id=\"eu_enter\"]/input[2]"));
        passField.SendKeys(pass);

        var submitBtn = _driver.FindElement(By.XPath("//*[@id=\"eu_enter\"]/input[3]"));
        submitBtn.Click();

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(drv => drv.FindElement(By.XPath("//*[@id=\"under-slider-menu\"]/div/a[3]")));
    }

    [Test]
    public async Task Uspevaemost_Successful()
    {
        await Login_Successful(Login, Pass);

        // клик на успеваемость
        var uspevaemost = _driver.FindElement(By.XPath("//*[@id=\"main-blocks\"]/div[1]/div[6]/a"));
        uspevaemost.Click();

        // ожидание прогрузки
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        IWebElement coursesDiv = null!;
        wait.Until(drv => coursesDiv = drv.FindElement(By.XPath("//*[@id=\"progress-analyst-all\"]/div[1]/div")));

        //найдено курсов больше чем 0
        var courses = coursesDiv.FindElements(By.TagName("a"));
        Assert.That(courses, Has.Count.GreaterThan(1));
    }

    [Test]
    public async Task Score_List_Book_Subject_Successful()
    {
        await Login_Successful(Login, Pass);

        // клик на успеваемость
        var uspevaemost = _driver.FindElement(By.XPath("//*[@id=\"main-blocks\"]/div[1]/div[6]/a"));
        uspevaemost.Click();

        // ожидание прогрузки
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        IWebElement scoreList = null!;
        wait.Until(drv => scoreList = drv.FindElement(By.XPath("//*[@id=\"under-slider-menu\"]/div/a[9]")));

        // клик на электронную зачетную книжку
        scoreList.Click();

        // ожидание прогрузки
        IWebElement scoreTable = null!;
        wait.Until(drv => scoreTable = drv.FindElement(By.XPath("//*[@id=\"students-record-book\"]/table/tbody")));

        //найдено дисциплин больше чем 0
        var courses = scoreTable.FindElements(By.TagName("tr"));
        Assert.That(courses, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task Score_List_Subject_Successful()
    {
        await Login_Successful(Login, Pass);

        // клик на успеваемость
        var uspevaemost = _driver.FindElement(By.XPath("//*[@id=\"main-blocks\"]/div[1]/div[6]/a"));
        uspevaemost.Click();

        // ожидание прогрузки
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        IWebElement scoreList = null!;
        wait.Until(drv => scoreList = drv.FindElement(By.XPath("//*[@id=\"under-slider-menu\"]/div/a[8]")));

        // клик на успеваемость за семестр
        scoreList.Click();

        // ожидание прогрузки
        IWebElement progress = null!;
        wait.Until(drv => progress = drv.FindElement(By.XPath("//*[@id=\"progress\"]")));

        //найдено предметов больше чем 0
        var subjects = progress.FindElements(By.ClassName("progress-row"));
        Assert.That(subjects, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task Control_Journal_Successful()
    {
        await Login_Successful(Login, Pass);

        // клик на успеваемость
        var uspevaemost = _driver.FindElement(By.XPath("//*[@id=\"main-blocks\"]/div[1]/div[6]/a"));
        uspevaemost.Click();

        // ожидание прогрузки
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        IWebElement scoreList = null!;
        wait.Until(drv => scoreList = drv.FindElement(By.XPath("//*[@id=\"under-slider-menu\"]/div/a[12]")));

        // клик на журнал текущего контроля
        scoreList.Click();

        // ожидание прогрузки
        IWebElement progress = null!;
        wait.Until(drv => progress = drv.FindElement(By.XPath("//*[@id=\"students-record-book\"]/table")));

        //найдено семестров больше чем 0
        var subjects = progress.FindElements(By.TagName("tbody"));
        Assert.That(subjects, Has.Count.GreaterThan(0));
    }

    [Test]
    public async Task Attendance_Journal_Successful()
    {
        await Login_Successful(Login, Pass);

        // клик на успеваемость
        var uspevaemost = _driver.FindElement(By.XPath("//*[@id=\"main-blocks\"]/div[1]/div[6]/a"));
        uspevaemost.Click();

        // ожидание прогрузки
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        IWebElement scoreList = null!;
        wait.Until(drv => scoreList = drv.FindElement(By.XPath("//*[@id=\"under-slider-menu\"]/div/a[13]")));

        // клик на посещение занятий
        scoreList.Click();

        // ожидание прогрузки
        IWebElement progress = null!;
        wait.Until(drv => progress = drv.FindElement(By.XPath("//*[@id=\"students-record-book\"]/table/tbody")));

        //найдено записей с посещениями больше чем 0
        var subjects = progress.FindElements(By.TagName("tr"));
        Assert.That(subjects, Has.Count.GreaterThan(0));
    }

    //todo: тест-кейсы убраны на гитхабе в целях сохранения конфинденциальности
    [TestCase("Фамилия Имя Отчество", new[] { 5, 5, 5, 5 })]
    public async Task Rating_Anketa_Successful(string teacherName, int[] inputRates)
    {
        await Login_Successful(Login, Pass);

        // клик на успеваемость
        var uspevaemost = _driver.FindElement(By.XPath("//*[@id=\"main-blocks\"]/div[1]/div[6]/a"));
        uspevaemost.Click();

        // ожидание прогрузки
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        IWebElement ratingAnketa = null!;
        wait.Until(drv => ratingAnketa = drv.FindElement(By.XPath("//*[@id=\"under-slider-menu\"]/div/a[11]")));

        // клик на оценку работы преподавателей
        ratingAnketa.Click();

        // ожидание прогрузки
        IWebElement teachersList = null!;
        wait.Until(drv => teachersList = drv.FindElement(By.XPath("//*[@id=\"teacher-rating\"]/div/div[1]/ul")));

        //получаем список учителей и ждем их загрузки
        var teachers = teachersList.FindElements(By.TagName("a"));
        Assert.That(teachers, Has.Count.GreaterThan(0));
        // нахоодим нужного учителя
        var teacher = teachers.FirstOrDefault(x => x.Text == teacherName);
        Assert.That(teacher, Is.Not.EqualTo(null));
        teacher.Click();

        // ожидание прогрузки
        IWebElement teacherLabel = null!;
        wait.Until(drv => teacherLabel = drv.FindElement(By.XPath("//*[@id=\"teacher-rating\"]/div/div[2]/div[1]")));
        Assert.That(teacherLabel.Text, Is.EqualTo(teacherName));

        // начало голосования
        _driver.FindElement(By.XPath("//*[@id=\"golos_result\"]/tbody/tr[5]/td[2]/a")).Click();
        await Task.Delay(1500); // тут ждем JS, поэтому без стратегии ожидания

        // прожимание нужных оценок
        for (var i = 0; i < 4; i++)
        {
            var rate = inputRates[i];
            var btn = _driver.FindElement(
                By.XPath($"//*[@id=\"golos_result\"]/tbody/tr[{1 + i}]/td[2]/div/span/div[{1 + rate}]/a"));
            btn.Click();
        }

        _driver.FindElement(By.XPath("//*[@id=\"golos_result\"]/tbody/tr[5]/td[2]/a")).Click();

        // ожидание прогрузки
        wait.Until(drv => teacherLabel = drv.FindElement(By.XPath("//*[@id=\"teacher-rating\"]/div/div[2]/div[1]")));
        Assert.That(teacherLabel.Text, Is.EqualTo(teacherName));

        // проверка оценок
        _driver.FindElement(By.XPath("//*[@id=\"golos_result\"]/tbody/tr[5]/td[2]/a")).Click();
        await Task.Delay(1500); // тут ждем JS, поэтому без стратегии ожидания
        for (var i = 0; i < 4; i++)
        {
            var rate = inputRates[i];
            var btn = _driver.FindElement(
                By.XPath($"//*[@id=\"golos_result\"]/tbody/tr[{1 + i}]/td[2]/div/span/div[{1 + rate}]"));
            var attribute = btn.GetDomAttribute("class");
            Assert.That(attribute, Does.Contain("star-rating-on"));
            if (rate == 5) continue;
            btn = _driver.FindElement(
                By.XPath($"//*[@id=\"golos_result\"]/tbody/tr[{1 + i}]/td[2]/div/span/div[{1 + rate + 1}]"));
            attribute = btn.GetDomAttribute("class");
            Assert.That(attribute, Does.Not.Contain("star-rating-on"));
        }
    }
}