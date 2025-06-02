using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using UserFormApp.Models;
using System.Linq;
using System;
//using UserFormApp.Data;

public class UserController : Controller
{




    private readonly AppDbContext _db;


    private bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserName"));
    }

    public UserController(AppDbContext db)


    {
        _db = db;
    }

    //public IActionResult List()
    //{
    //    var users = _db.Users.ToList();  // ✅ 從資料庫讀出所有會員
    //    return View(users);              // 傳給 List.cshtml 顯示
    //}



    [HttpGet]
    public IActionResult Register()
    {
        //    var rand = new Random();
        //    int a = rand.Next(1, 10);
        //    int b = rand.Next(1, 10);

        //    ViewBag.CaptchaQuestion = $"請輸入: {a}+{b}=??";
        //    TempData["CaptchaAnswer"] = (a + b).ToString();   //暫存正確答案
        SetNewCaptcha();
      return View();

    }

    private void SetNewCaptcha()
    {
        var rand = new Random();
        int a = rand.Next(1, 10);
        int b = rand.Next(1, 10);
        ViewBag.CaptchaQuestion = $"請輸入:{a}+{b}=??";
        TempData["CaptchaAnswer"] = (a + b).ToString();

       

    }



    [HttpPost]
    public IActionResult Register(UserModel user, string CaptchaInput)
    {
        try
        {
            var correctAnswer = TempData["CaptchaAnswer"] as string;
            if (CaptchaInput != correctAnswer)

            {
                ViewBag.CaptchaError = "驗證碼錯誤, 請重新輸入";

                var rand = new Random();
                int a = rand.Next(1, 10);
                int b = rand.Next(1, 10);
                ViewBag.CaptchaQuestion = $"請輸入:{a}+{b}=??";
                TempData["CaptchaAnswer"] = (a + b).ToString();

                return View(user);
            }

            if (!ModelState.IsValid)
            {
                var rand = new Random();
                int a = rand.Next(1, 10);
                int b = rand.Next(1, 10);
                ViewBag.CaptchaQuestion = $"請輸入:{a}+{b}=??";
                TempData["CaptchaAnswer"] = (a + b).ToString();

                return View(user);
            }
        }

        catch(Exception ex)
        {
            Console.WriteLine("註冊錯誤:" + ex.Message);
            ViewBag.Error = "註冊時發生錯誤,請稍後再試。";
            SetNewCaptcha();
            return View(user);

        }

        try
        {
            //密碼加密
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            System.Diagnostics.Debug.WriteLine("✅ 原始密碼：" + user.ConfirmPassword);
            System.Diagnostics.Debug.WriteLine("✅ 加密後密碼：" + user.Password);



            _db.Users.Add(user);
            _db.SaveChanges();

            //加入登入 Session 
            //要先設定Session 再 Redirect 否則沒辦法登入 會失效

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.Name);

            TempData["Message"] = $"歡迎{user.Name},註冊成功 !";


            //ViewBag.Message = $"歡迎{user.Name} 註冊成功";

            //導向會員中心
            return RedirectToAction("MemberCenter");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ 註冊錯誤: " + ex.Message);
            ViewBag.Error = "註冊時發生錯誤，請稍後再試。";
            SetNewCaptcha();
            return View(user);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {

        try
        {
            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            Console.WriteLine("使用者輸入的Email:" + email);
            Console.WriteLine("使用者輸入的輸入密碼:" + password);
            Console.WriteLine("資料庫密碼:" + user ?. Password);
         
           


            if (user != null )
            {
                bool isMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);
                Console.WriteLine("密碼是否匹配:" + isMatch);

                if(isMatch)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserName", user.Name);

                    TempData["Message"] = $"登入成功,歡迎回來{user.Name} !";
                    return RedirectToAction("MemberCenter"); // 轉後仍能顯示訊息

                }

                else
                {
                    TempData["Message"] = "密碼錯誤";
                    return RedirectToAction("Login");
                }

            }

            else
            {
                TempData["Message"] = "登入失敗,帳號或密碼錯誤";
                return RedirectToAction("Login");  //也能轉跳回登入頁再顯示錯誤
            }

        }

        catch (Exception ex)
        {
            TempData["Message"] = "❌ 登入過程發生錯誤，請稍後再試。";
            return RedirectToAction("Login");
        }

        



    }

    [HttpGet]

    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // ✨ 清除所有 Session 資料
        return RedirectToAction("Login");
    }


    //從Session 取得UserName(登入時才會有)
    //如果null 或 空字串 就強制導向Login頁面
    //否則才會顯示會員中心畫面
    public IActionResult MemberCenter()
    {
        var name = HttpContext.Session.GetString("UserName");

        if (string.IsNullOrEmpty(name))
        {
            return RedirectToAction("Login");
        }

        ViewBag.Name = name;
        return View();
    }

    public IActionResult List(string Keyword, string sort, int page = 1)
    {

        //登入檢查
        var name = HttpContext.Session.GetString("UserName");

        if (string.IsNullOrEmpty(name))
        {
           return RedirectToAction("Login");
        }

        int pageSize = 3;

        var users = _db.Users.AsQueryable(); // ✅ 這才是資料清單

        if (!string.IsNullOrEmpty(Keyword))
        {
            users = users.Where(u => u.Name.Contains(Keyword));
        }


        users = sort switch
        {
            "name_desc" => users.OrderByDescending(u => u.Name),
            "age" => users.OrderBy(u => u.Age),
            "age_desc" => users.OrderByDescending(u => u.Age),
            _ => users.OrderBy(u => u.Name)

        };

        int totalCount = users.Count();
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.CurrentPage = page;
        ViewBag.Keyword = Keyword;
        ViewBag.Sort = sort;

        var pageUsers = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View(pageUsers);





    }
    [HttpGet]

    public IActionResult Edit (int id)
    {

        if (!IsLoggedIn())
        {
            return RedirectToAction("Login");
        }



        var user = _db.Users.FirstOrDefault(u=> u.Id ==id);
        if (user == null)
        {
            return NotFound();

        }

        return View(user);
    }



    [HttpPost ]

    public IActionResult Edit (UserModel user)
    {
        ModelState.Remove("Password");
        ModelState.Remove("ConfirmPassword");

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var existingUser = _db.Users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser == null) return NotFound();

        existingUser.Name = user.Name;
        existingUser.Age = user.Age;
        existingUser.Email = user.Email;

        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            existingUser.Password = user.Password;
        }

        _db.SaveChanges();

        TempData["Message"] = "會員資料修改成功";
        return RedirectToAction("List");
    }
 
    [HttpGet]
    public IActionResult Delete(int id)
    {

        if (!IsLoggedIn())
        {
            return RedirectToAction("Login");
        }



        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user); // 顯示刪除確認畫面 Delete.cshtml
    }

    [HttpPost]
    [ActionName("Delete")]
    public IActionResult DeleteConfirmed(int Id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == Id);
        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["Message"] = "會員資料已刪除";

        }
        return RedirectToAction("List");

    }




}