using System.Diagnostics;
using System.Text.Json;
using AspKnP231.Data;
using AspKnP231.Models;
using AspKnP231.Models.Home;
using AspKnP231.Services.Hash;
using AspKnP231.Services.Scoped;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspKnP231.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IStorageService _scopedService;
        private readonly IHashService _hashService;           // Інжекція сервісу "через конструктор" -
                                                              // рекомендований спосіб, передбачає 
        public HomeController(IHashService hashService, IStorageService scopedService, DataContext dataContext)
        {                                                     // readonly поле - посилання на сервіс та 
            _hashService = hashService;                       // параметр(и) конструктора того ж типу даних
            _scopedService = scopedService;
            _dataContext = dataContext;
        }


        public IActionResult Forms()
        {
            HomeFormsViewModel viewModel = new();
            if (HttpContext.Session.Keys.Contains(nameof(HomeFormsFormModel)))
            {
                // є збережені у сесії дані, тоді відновлюємо, використовуємо та видаляємо
                viewModel.FormModel = JsonSerializer.Deserialize<HomeFormsFormModel>(
                    HttpContext.Session.GetString(nameof(HomeFormsFormModel))!
                );

                ModelStateDictionary modelState = new();

                JsonElement savedState = JsonSerializer.Deserialize<JsonElement>(
                    HttpContext.Session.GetString(nameof(ModelState))!
                )!;
                foreach (var item in savedState.EnumerateObject())
                {
                    var errors = item.Value.GetProperty("Errors");
                    if (errors.GetArrayLength() > 0)
                    {
                        foreach (var err in errors.EnumerateArray())
                        {
                            modelState.AddModelError(item.Name, err.GetProperty("ErrorMessage").GetString()!);
                        }
                    }
                }
                viewModel.FormModelState = modelState;

                HttpContext.Session.Remove(nameof(HomeFormsFormModel));
                HttpContext.Session.Remove(nameof(ModelState));
            }

            return View(viewModel);
        }

        // метод для прийому даних форми, збереження у сесії та передачі редирект
        public IActionResult FormReceiver(HomeFormsFormModel formModel)
        {
            // валідація форми - знаходиться у ModelState
            // додатково до неї проводимо перевірки, що не зазначаються атрибутами моделі
            if (_dataContext.UserAccesses.Any(ua => ua.Login == formModel.UserLogin))
            {
                ModelState.AddModelError("user-login", "Даний логін вже у вжитку");
            }
            /* Реалізувати перевірку пароля на надійність:
             * - довжина щонайменше 6 символів
             * - містить принаймні одну цифру
             * - містить принаймні одну маленьку літеру
             * - містить принаймні одну велику літеру
             * - містить принаймні один спецсимвол (не-літера, не-цифра)
             */
            HttpContext.Session.SetString(
                nameof(ModelState),
                JsonSerializer.Serialize(ModelState)
            );

            HttpContext.Session.SetString(
                nameof(HomeFormsFormModel),
                JsonSerializer.Serialize(formModel)
            );

            return RedirectToAction(nameof(Forms));   //  /Home/Forms - формує ASP
        }




        public IActionResult Middleware()
        {
            return View();
        }

        public IActionResult IoC()
        {

            // використовуємо сервіс і передаємо дані до представлення
            ViewData["hash"] = _hashService.Digest("123");
            ViewData["hashCode"] = _hashService.GetHashCode();
            ViewData["ControllerScopedHash"] = _scopedService.GetHashCode();
            return View();
        }

        // Зв'язування моделі відбувається коли ми її зазначаємо вхідними параметром Action
        // В старих ASP, якщо модель не є обов'язковою, то необхідно
        // зазначати Nullable (HomeModelsFormModel?)
        public IActionResult Models(DemoMiddleware formModel)
        {
            // Особливість нових ASP - модель форми, як об'єкт, створюється
            // у будь-якому випадку, навіть якщо немає даних від форми
            // З метою розрізнення випадків наявності/відсутності даних вводиться
            // елемент форми, що відповідає за кнопку надсилання форми.

            HomeModelsViewModel viewModel = new();
            if (formModel.UserButton != null)
            {
                viewModel.FormModel = formModel;
            }

            return View(viewModel);
        }
        /* Ä.Ç. Çðîáèòè ñòîð³íêó ç ôîðìîþ ðåºñòðàö³¿ íîâîãî êîðèñòóâà÷à
         * Îïèñàòè óñ³ íåîáõ³äí³ ìîäåë³
         */
        public IActionResult Razor()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult UnLayout()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new UserSignupFormModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}