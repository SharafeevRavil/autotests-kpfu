using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace OzonTest;

public class WbComplexTest
{
    private IWebDriver _driver;

    [SetUp]
    public void Setup()
    {
    }

    [SetUp]
    public void SetUp()
    {
        var options = new ChromeOptions();
        // options.Proxy = new Proxy { HttpProxy = "203.190.117.97:8076" };
        options.AddArgument("--start-maximized"); // задание размера окна
        var userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36";
        options.AddArgument($"user-agent={userAgent}");
        if (!Debugger.IsAttached)
        {
            options.AddArgument("headless"); // запуск браузера в фоновом режиме
        }

        // _driver = new EdgeDriver();
        _driver = new ChromeDriver(options);

        _driver.Navigate().GoToUrl("https://www.wildberries.ru/");

        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        wait.Until(drv => drv.FindElement(By.CssSelector("button[data-wba-header-name=\"Catalog\"]")));
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private void RealWait(By by)
    {
        if (!Debugger.IsAttached)
        {
            var wait = new DefaultWait<IWebDriver>(_driver)
            {
                Timeout = TimeSpan.FromSeconds(5),
                PollingInterval = TimeSpan.FromMilliseconds(200)
            };
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
            wait.Until(drv => drv.FindElement(by));
        }
        else
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            wait.Until(ExpectedConditions.ElementIsVisible(by));
        }
    }

    [Test]
    public async Task ComplexTestCase()
    {
        // открыть кнопку каталога 
        _driver.FindElement(By.CssSelector("button[data-wba-header-name=\"Catalog\"]")).Click();
        // навести мышь на раздел "Электроника"
        RealWait(By.CssSelector("a[href=\"/catalog/elektronika\"]"));
        var digital = _driver.FindElement(By.CssSelector("a[href=\"/catalog/elektronika\"]"));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", digital);
        new Actions(_driver).MoveToElement(digital).Perform();
        // перейти в раздел "Музыка и видео"
        RealWait(By.CssSelector("a[href=\"/catalog/elektronika/muzyka-i-video\"]"));
        _driver.FindElement(By.CssSelector("a[href=\"/catalog/elektronika/muzyka-i-video\"]")).Click();
        // открыть фильтры
        RealWait(By.CssSelector("button.dropdown-filter__btn--all"));
        _driver.FindElement(By.CssSelector("button.dropdown-filter__btn--all")).Click();
        // среди открытых фильтров найти "Категория"
        RealWait(By.CssSelector(".filters-desktop__list"));
        var openedFilters = _driver.FindElements(By.CssSelector("div.filters-desktop__item"));
        var categoryFilter = openedFilters.First(x =>
            x.FindElement(By.CssSelector(".filters-desktop__item-title")).Text.Contains("Категория"));
        // внутри категории найти "Виниловые пластинки"
        var categoryItems =
            categoryFilter.FindElements(By.CssSelector("li.filter__item > div > .checkbox-with-text__text"));
        categoryItems.First(x => x.Text == "Виниловая пластинка").Click();
        // применить фильтры
        _driver.FindElement(By.CssSelector("button.filters-desktop__btn-main")).Click();


        // добавить в корзину
        var (productName, productPrice) = await AddProductToBasket();
        // переход в корзину
        _driver.FindElement(By.CssSelector(".j-item-basket > a")).Click();
        await Task.Delay(1000); // идиотская анимация пролистывания цены - из-за нее портится тест
        RealWait(By.CssSelector(".basket-page__blocks-wrap"));
        // проверить что товар в корзине
        CheckBasketAmount(1);
        // проверить товар в корзине
        CheckBasketProduct(0, productName, productPrice);

        // вернуться
        await _driver.Navigate().BackAsync();
        await _driver.Navigate().BackAsync();

        // добавить в корзину
        var (productName2, productPrice2) = await AddProductToBasket();
        // переход в корзину
        _driver.FindElement(By.CssSelector(".j-item-basket > a")).Click();
        await Task.Delay(1000); // идиотская анимация пролистывания цены - из-за нее портится тест
        RealWait(By.CssSelector(".basket-page__blocks-wrap"));
        // проверить что товар в корзине
        CheckBasketAmount(2);
        // проверить товар в корзине
        CheckBasketProduct(0, productName2, productPrice2);
        CheckBasketProduct(1, productName, productPrice);

        //проверить сумму
        var sumStr = _driver.FindElement(By.CssSelector(".b-top__total > span > span[data-link]")).Text;
        var sum = int.Parse(sumStr.Replace(" ", "").Replace("\u00a0", "").Replace("\u20bd", ""));
        Assert.That(sum, Is.EqualTo(productPrice + productPrice2));

        // очистить корзину
        foreach (var delBtn in _driver.FindElements(By.CssSelector(".btn__del")))
        {
            delBtn.Click();
        }

        var emptyBasket = _driver.FindElement(By.CssSelector(".basket-empty__title")).Text;
        Assert.That(emptyBasket, Is.EqualTo("В корзине пока пусто"));
    }

    private async Task<(string productName, int productPrice)> AddProductToBasket()
    {
        // проверить что открылся список товаров
        await Task.Delay(1000); // js
        RealWait(By.CssSelector("[data-link=\"html{spaceFormatted:pagerModel.totalItems}\"]"));
        // проверить что количество товаров больше 0
        var totalItemsText = _driver
            .FindElement(By.CssSelector("[data-link=\"html{spaceFormatted:pagerModel.totalItems}\"]")).Text;
        var totalItems = int.Parse(totalItemsText.Replace(" ", ""));
        Console.WriteLine($"Total items: {totalItems}");
        Assert.That(totalItems, Is.GreaterThan(0));
        var products = _driver.FindElements(By.CssSelector(".product-card-list > article"));
        Console.WriteLine($"On page items: {products.Count}");
        Assert.That(products, Is.Not.Empty);
        // выбрать случайный товар
        var random = new Random().Next(0, products.Count);
        Console.WriteLine($"Random product: {random}");
        products[random].Click();
        // тут 18+ товар может быть
        await Task.Delay(1000);
        var confirmAgeBtn = _driver.FindElements(By.CssSelector(".popup__btn-main"));
        if (confirmAgeBtn.Count > 0) confirmAgeBtn[0].Click();
        // проверить что открылась страница товара
        RealWait(By.CssSelector(".product-page__title"));
        var productName = _driver.FindElement(By.CssSelector(".product-page__title")).Text;
        RealWait(By.CssSelector(".product-page__aside-container"));
        var priceHolder = _driver.FindElement(By.CssSelector(".product-page__aside-container"));
        var productPriceText = priceHolder.FindElement(By.CssSelector(".price-block__final-price")).Text;
        // var productPriceText = _driver.FindElement(By.CssSelector(".price-block__final-price")).Text;
        var productPrice =
            int.Parse(productPriceText.Replace(" ", "").Replace("\u00a0", "")
                .Replace("\u20bd", "")); // "\u00a0" это " " "\u20bd" это "₽"
        Console.WriteLine($"Selected product: {productName}. Price: {productPrice}");
        // добавить
        _driver.FindElement(By.CssSelector("div.product-page__order-buttons > div > div > div > button")).Click();
        return (productName, productPrice);
    }

    private void CheckBasketAmount(int amount)
    {
        var basketAmountStr = _driver.FindElement(By.CssSelector(".accordion__goods-count")).Text;
        var basketAmount = int.Parse(string.Concat(basketAmountStr.Where(char.IsDigit)));
        Assert.That(basketAmount, Is.EqualTo(amount));
    }

    private void CheckBasketProduct(int productIndex, string productName, int productPrice)
    {
        var basketProductName = _driver.FindElements(By.CssSelector(".good-info__good-name"))[productIndex].Text;
        Assert.That(basketProductName, Is.EqualTo(productName));
        var basketProductPriceStr = _driver.FindElements(By.CssSelector(".list-item__price-new"))[productIndex].Text;
        var basketProductPrice =
            int.Parse(basketProductPriceStr.Replace(" ", "").Replace("\u00a0", "").Replace("\u20bd", ""));
        Assert.That(basketProductPrice, Is.EqualTo(productPrice));
    }
}