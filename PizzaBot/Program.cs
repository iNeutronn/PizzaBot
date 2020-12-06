using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.IO;

namespace PizzaBot
{
    class Program

    {
        static ITelegramBotClient bot;
        const byte MinAddresLength = 7;
        const long id_admin = 686943660;
        static readonly Dictionary<long, User> Users = new Dictionary<long, User>();
        static readonly DB DB = new DB();


        static void Main()
        {
           
            Console.Title = "AdminPanel PizzaBot";
            bot = new TelegramBotClient("856744989:AAEOp1PYFs_PJq2gjGuGoD5QRVw6MQuf1oo")
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            var me = bot.GetMeAsync().Result;
            Console.WriteLine($"id:{me.Id} name:{me.FirstName + me.LastName}");
            bot.OnMessage += Resive;
            bot.OnCallbackQuery += CallBack;
            bot.StartReceiving();

            while (true)
            {
                Console.ReadLine();
            }
        }
        private static void CallBack(object sender, CallbackQueryEventArgs e)
        {
            Message m = e.CallbackQuery.Message;
            string[] s = e.CallbackQuery.Data.Split();
            if (e.CallbackQuery.Data == "Назад")
            {
                Send("OK. Чим можу допомогти?", e.CallbackQuery.Message.Chat.Id, Menu.main_menu);
                Users[e.CallbackQuery.Message.Chat.Id].Status = UserStat.WaitCommand;
            }
            else if (s[0] == "AddToShopCard")
            {
                int id = int.Parse(s[1]);
                Users[m.Chat.Id].order.Add(id);
                Send("Ще щось?", m.Chat.Id, Menu.Order(Users[m.Chat.Id].order.Count + Users[m.Chat.Id].orderCreatePizza.Count));
            }
        }
        private static void WaitingCommand(Message m)
        {
            switch (m.Text)
            {
                case "/start":
                    Send("Привіт! \n чим можу допомогти?", m.Chat.Id, Menu.main_menu);
                    break;
                case "Надіслати повідомлення адміністрації":
                    Send("Що ви хочите повідомити?", m.Chat.Id, Menu.back);
                    Users[m.Chat.Id].Status = UserStat.SendingToAdm;
                    break;
                case "Наші контакти":
                    Send("м.Бердичів", m.Chat.Id, Menu.main_menu);
                    break;
                case "Ознайомитись з меню":
                    Send("Виберіть групу страв", m.Chat.Id, Menu.dish_type);
                    Users[m.Chat.Id].Status = UserStat.ChoosingGroup;
                    break;
                case "Замовити щось":
                    Users[m.Chat.Id].Status = UserStat.ChoosingOrgerGroup;
                    Send("Виберіть групу страв", m.Chat.Id, Menu.dish_type);
                    break;
                case "Змінити інформацію":
                    Send("Що саме ви хочите змінити?", m.Chat.Id, Menu.ChangeInf);
                    Users[m.Chat.Id].Status = UserStat.ChengingInf;
                    break;
                default:
                    Send("Що?", m.Chat.Id);
                    break;
            }
        }
        static async void AddToBD(Message m)
        {
            DB.OpenAsync();
            SqlCommand chekUser = new SqlCommand($"SELECT * FROM [Users] WHERE chatid = {m.Chat.Id}", DB.GetConnection());
            while (DB.GetConnection().State == ConnectionState.Connecting) ;
            SqlDataReader r = await chekUser.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                r.Close();
                DB.Close();
                return;
            }
            r.Close();
            string c = $"INSERT INTO [Users] (chatid,Name)VALUES({m.Chat.Id},'{ m.Chat.FirstName + m.Chat.LastName}')";

            SqlCommand command = new SqlCommand(c, DB.GetConnection());
            await command.ExecuteNonQueryAsync();
            DB.Close();
        }
        public async static void Resive(object sender, MessageEventArgs e)
        {
            //try
            //{
            Message m = e.Message;
            if (!Users.ContainsKey(m.Chat.Id))
            {
                Users.Add(m.Chat.Id, new User());

                await Task.Run(() => AddToBD(m));
            }
            Users[m.Chat.Id].start = DateTime.Now;
            switch (Users[m.Chat.Id].Status)
            {
                case UserStat.WaitCommand:
                    WaitingCommand(m);
                    break;
                case UserStat.SendingToAdm:
                    SendToAdmin(m);
                    break;
                case UserStat.ChoosingGroup:
                    ChoosingGropup(m);
                    break;
                case UserStat.ChoosingOrgerGroup:
                    ChoosingOrgerGroup(m);
                    break;
                case UserStat.EditOrder:
                    EditOrder(m);
                    break;
                case UserStat.EnteringPhone:
                    EnterPhone(m, false);
                    break;
                case UserStat.EnteringAddresToOrd:
                    EnterAdress(m, true);
                    break;
                case UserStat.EnteringAddres:
                    EnterAdress(m, false);
                    break;
                case UserStat.ConfigingOrder:
                    ConfigingOrder(m);
                    break;
                case UserStat.ChengingInf:
                    ChangeInf(m);
                    break;
                case UserStat.EnteringPhoneToOrd:
                    EnterPhone(m, true);
                    break;
                case UserStat.CreatingPizza:
                    CraatingPizza(m);
                    break;
            }
            //}
            //catch
            //{
            //    Send("Error", e.Message.Chat.Id);
            //}
            GC.Collect();
        }
        private static void CraatingPizza(Message m)
        {
            switch (Users[m.Chat.Id].PizzaStat)
            {
                case PizzaStat.ChoosingSize:
                    Users[m.Chat.Id].currentpizza = $"Своя піца: Розмір: {m.Text};";
                    Send("Виберіть соус для своєї піци", m.Chat.Id, Menu.Sauce);
                    Users[m.Chat.Id].PizzaStat = PizzaStat.ChoosingSauce;
                    break;
                case PizzaStat.ChoosingSauce:
                    Users[m.Chat.Id].currentpizza += $" Соус: {m.Text};";
                    Send("Виберіть сир для своєї піци", m.Chat.Id, Menu.Cheese);
                    Users[m.Chat.Id].PizzaStat = PizzaStat.ChossingCheese;
                    break;
                case PizzaStat.ChossingCheese:
                    Users[m.Chat.Id].currentpizza += $"Сир: {m.Text}; Дoдaтки: ";
                    Send("Виберіть додатки для своєї піци", m.Chat.Id, Menu.Additions);
                    Users[m.Chat.Id].PizzaStat = PizzaStat.ChossingAddition;
                    break;
                case PizzaStat.ChossingAddition:
                    if (m.Text == "Додати піцу")
                        ConfigingPizza(m);
                    else
                    {
                        Users[m.Chat.Id].addons.Add(m.Text);
                        Send("Ще щось?", m.Chat.Id, Menu.Additions);
                    }
                    break;
                case PizzaStat.ConfigingOrger:
                    switch (m.Text)
                    {
                        case "Так оформити замовлення":
                            CheckPhone(m);
                            break;
                        case "Ні продовжити покупки":
                            Send("Чим можу допомогти", m.Chat.Id, Menu.main_menu);
                            Users[m.Chat.Id].Status = UserStat.WaitCommand;
                            break;
                    }
                    break;

            }

        }
        private static void ConfigingPizza(Message m)
        {
            string pizza = Users[m.Chat.Id].currentpizza;
            List<string> add = Users[m.Chat.Id].addons;
            add.Sort();
            for (int i = 0; i < add.Count; i++)
            {
                int counter = 1;
                try
                {
                    while (add[i] == add[i + 1])
                    {
                        counter++;
                        add.RemoveAt(i + 1);
                    }
                }
                catch
                {

                }
                if (counter > 1)
                    add[i] += $" ({counter})";
            }
            for (int i = 0; i < add.Count; i++)
                if (i == add.Count - 1)
                    pizza += add[i];
                else
                    pizza += add[i] + ',';
            Users[m.Chat.Id].orderCreatePizza.Add(pizza);
            Users[m.Chat.Id].currentpizza = null;
            Users[m.Chat.Id].addons.Clear();
            Send($"Oформити замовлення ({pizza}) зараз чи замовити ще щось?", m.Chat.Id, Menu.ConfigCreate);
            Users[m.Chat.Id].PizzaStat = PizzaStat.ConfigingOrger;
        }
        private static void ChangeInf(Message m)
        {
            switch (m.Text)
            {
                case "Назад":
                    Send("OK. Чим можу допомогти?", m.Chat.Id, Menu.main_menu);
                    Users[m.Chat.Id].Status = UserStat.WaitCommand;
                    break;
                case "Змінити номер телефону":
                    Send("Введіть номер телефону", m.Chat.Id, Menu.back);
                    Users[m.Chat.Id].Status = UserStat.EnteringPhone;
                    break;
                case "Змінити адрес доставки":
                    Send("Введіть адрес", m.Chat.Id, Menu.back);
                    Users[m.Chat.Id].Status = UserStat.EnteringAddres;
                    break;

            }
        }
        private async static void ConfigingOrder(Message m)
        {
            switch (m.Text)
            {
                case "Ні відмінити":
                    Send("OK. Чим можу допомогти?", m.Chat.Id, Menu.main_menu);
                    Users[m.Chat.Id].Status = UserStat.WaitCommand;
                    break;
                case "Так підтвердити замовлення":
                    Send("Ваше замовлення прийняте", m.Chat.Id, Menu.main_menu);
                    string ord = GetOrder(m);
                    DB.OpenAsync();
                    while (DB.GetConnection().State != ConnectionState.Open) ;
                    SqlCommand command = new SqlCommand($"SELECT * FROM [Users] WHERE [chatid] = {m.Chat.Id}", DB.GetConnection());
                    SqlDataReader r = await command.ExecuteReaderAsync();
                    await r.ReadAsync();
                    char a = '"';
                    string text = $"Нове замовлення {a}{ord}{a} \n Адрес доставки: {r["addres"]} \n Контактрий номер телефону: {r["phonenumber"]}";
                    Send(text, id_admin);
                    await r.CloseAsync();
                    DB.Close();
                    Users[m.Chat.Id].Clear_card();
                    break;
            }
        }
        private async static void EnterAdress(Message m, bool b)
        {
            if (m.Text.Length < MinAddresLength)
            {
                Send("Помилка.Ввведіть свій адресс!", m.Chat.Id);
                return;
            }
            DB.OpenAsync();
            while (DB.GetConnection().State != ConnectionState.Open) ;
            SqlCommand command = new SqlCommand($"UPDATE [Users] SET [addres] = '{m.Text}' WHERE [chatid] = '{m.Chat.Id}'", DB.GetConnection());
            await command.ExecuteNonQueryAsync();
            DB.Close();
            if (b)
                ConfigmOrder(m);
            else
            {
                Send("Ваш адрес змінено", m.Chat.Id, Menu.main_menu);
                Users[m.Chat.Id].Status = UserStat.WaitCommand;
            }
        }
        private async static void ConfigmOrder(Message m)
        {
            string order = GetOrder(m);

            DB.OpenAsync();
            while (DB.GetConnection().State != ConnectionState.Open) ;
            SqlCommand command = new SqlCommand($"SELECT * FROM [Users] WHERE [chatid] = {m.Chat.Id}", DB.GetConnection());
            SqlDataReader r = command.ExecuteReader();
            await r.ReadAsync();
            char a = '"';
            string text = $"Ви хочите замовити {a}{order}{a} \n Адрес доставки: {r["addres"]} \n Контактрий номер телефону: {r["phonenumber"]}";
            r.Close();
            DB.Close();
            Users[m.Chat.Id].Status = UserStat.ConfigingOrder;
            Send(text, m.Chat.Id, Menu.ConfigmNo);
        }
        private static string GetOrder(Message m)
        {
            Users[m.Chat.Id].order.Sort();
            List<string> order = new List<string>();
            int prise = 0;
            DB.Close();
            foreach (int id in Users[m.Chat.Id].order)
            {
                DB.OpenAsync();
                while (DB.GetConnection().State != ConnectionState.Open) ;
                SqlCommand command = new SqlCommand($"SELECT * FROM [Dishes] WHERE [id] = '{id}'", DB.GetConnection());
                SqlDataReader r = command.ExecuteReader();
                r.Read();
                order.Add(r["Name"].ToString());
                prise += int.Parse(r["prise"].ToString());
                r.Close();
                DB.Close();
            }
            order.AddRange(Users[m.Chat.Id].orderCreatePizza);
            for (int i = 0; i < order.Count; i++)
            {
                string dish = order[i];
                int startindex = 0;
                for (int j = dish.Length - 1; j >= 0; j--)
                {
                    if (dish[j] == ' ')
                        continue;
                    else
                    {
                        startindex = j;
                        break;
                    }
                }
                string output = "";
                for (int j = 0; j <= startindex; j++)
                {
                    output += dish[j];
                }
                order[i] = output;
            }
            order.Sort();
            for (int i = 0; i < order.Count; i++)
            {
                int counter = 1;
                try
                {
                    while (order[i] == order[i + 1])
                    {
                        counter++;
                        order.RemoveAt(i + 1);
                    }
                }
                catch
                {

                }
                if (counter > 1)
                    order[i] += $" ({counter}шт.)";
            }
            string Output = "";
            for (int i = 0; i < order.Count; i++)
                if (i == order.Count - 1)
                    Output += order[i];
                else
                    Output += order[i] + ", ";
            return Output;
        }
        private async static void EnterPhone(Message m, bool b)
        {
            string input = m.Text;
            if (input.Length != 10 & input.Length != 13)
            {
                Send("Помилка. \n Введіть свій номер телефону", m.Chat.Id, Menu.back);
                return;
            }
            if (input[0] == '+')
            {
                string s = "";
                for (int i = 1; i < input.Length; i++)
                    s += input[i];
                input = s;
            }
            try
            {
                ulong.Parse(input);
            }
            catch
            {
                Send("Помилка. \n Введіть свій номер телефону", m.Chat.Id, Menu.back);
                return;
            }
            DB.OpenAsync();
            while (DB.GetConnection().State != ConnectionState.Open) ;
            SqlCommand command = new SqlCommand($"UPDATE [Users] SET [Phonenumber] = '{input}' WHERE [chatid] = '{m.Chat.Id}'", DB.GetConnection());
            await command.ExecuteNonQueryAsync();
            DB.Close();
            if (b)
                CheckAddres(m);
            else
            {
                Send("Ваш номер телефону змінено", m.Chat.Id, Menu.main_menu);
                Users[m.Chat.Id].Status = UserStat.WaitCommand;
            }
        }
        private static void EditOrder(Message m)
        {
            string[] c = m.Text.Split();
            if (m.Text == "Очистити кошик")
            {
                Send("Кошик очищений", m.Chat.Id, Menu.Order(0));
                Users[m.Chat.Id].order = new List<int>();
            }
            else if (m.Text == "Назад")
            {
                Users[m.Chat.Id].Status = UserStat.ChoosingOrgerGroup;
                Send("Виберіть групу страв", m.Chat.Id, Menu.dish_type);

            }
            else if (c[0] == "Оформити")
            {
                CheckPhone(m);
            }
            else if (m.Text == "Виберайте те що досмаку)")
                Send("Виберіть хоча б 1 страву", m.Chat.Id,Menu.Order(Users[m.Chat.Id].order.Count + Users[m.Chat.Id].orderCreatePizza.Count)) ;
        }
        private async static void CheckPhone(Message m)
        {
            DB.OpenAsync();
            SqlCommand c = new SqlCommand($"SELECT * FROM [Users] WHERE chatid = '{m.Chat.Id}'", DB.GetConnection());
            SqlDataReader r = await c.ExecuteReaderAsync();
            r.Read();
            if (r["phonenumber"].ToString() == "")
            {
                Send("Введіть свій номер телефону", m.Chat.Id, Menu.back);
                Users[m.Chat.Id].Status = UserStat.EnteringPhoneToOrd;
                r.Close();
                DB.Close();
            }
            else
            {
                r.Close();
                DB.Close();
                CheckAddres(m);
            }
        }
        private async static void CheckAddres(Message m)
        {
            DB.OpenAsync();
            SqlCommand c = new SqlCommand($"SELECT * FROM [Users] WHERE [chatid] = '{m.Chat.Id}'", DB.GetConnection());
            SqlDataReader r = await c.ExecuteReaderAsync();
            r.Read();
            if (r["addres"].ToString() == "")
            {
                Send("Введіть свій адрес", m.Chat.Id, Menu.back);
                Users[m.Chat.Id].Status = UserStat.EnteringAddresToOrd;
                r.Close();
                DB.Close();
            }
            else
            {
                r.Close();
                DB.Close();
                ConfigmOrder(m);
            }
        }
        private static void ChoosingOrgerGroup(Message m)
        {
            switch (m.Text)
            {
                case "Піци":
                    SendPropoz(GroupDish.Pizza, m);
                    break;
                case "Напої":
                    SendPropoz(GroupDish.Drinks, m);
                    break;
                case "Своя піца":
                    Send("Гаразд виберіть розмір піци", m.Chat.Id, Menu.Size);
                    Users[m.Chat.Id].Status = UserStat.CreatingPizza;
                    Users[m.Chat.Id].PizzaStat = PizzaStat.ChoosingSize;
                    break;
                case "Інше":
                    SendPropoz(GroupDish.Other, m);
                    break;
                case "Назад":
                    Users[m.Chat.Id].Status = UserStat.WaitCommand;
                    Send("Чим можу допомогти?", m.Chat.Id, Menu.main_menu);
                    break;
                default:
                    Send("Що?", m.Chat.Id);
                    break;
            }


        }
        private async static void SendPropoz(GroupDish group, Message m)
        {
            DB.OpenAsync();
            SqlCommand command;
            if (group == GroupDish.Pizza)
                command = new SqlCommand("SELECT * FROM[Dishes] WHERE type = 'Pizza'", DB.GetConnection());
            else if (group == GroupDish.Drinks)
                command = new SqlCommand("SELECT * FROM[Dishes] WHERE type = 'Drinks'", DB.GetConnection());
            else
                command = new SqlCommand("SELECT * FROM[Dishes] WHERE type = 'Other'", DB.GetConnection());
            try
            {
                SqlDataReader reader = await command.ExecuteReaderAsync();
                int counter = 0;
                while (await reader.ReadAsync())
                {
                    counter++;
                    if (reader["LinkIMG"].ToString() == "")
                    {
                        string text = String.Format($"{counter}. {reader["Name"]} \n Ціна: {reader["prise"]} грн \n {reader["Deskription"]}");
                        Send(text, m.Chat.Id, Menu.AddToAhopCart(int.Parse(reader["id"].ToString())));
                    }
                    else
                        try
                        {
                            string text = String.Format($"{counter}. {reader["Name"]} \n Ціна: {reader["prise"]} грн \n {reader["Deskription"]}");
                            SendPhoto(reader["LinkIMG"].ToString(), m.Chat.Id, text, Menu.AddToAhopCart(int.Parse(reader["id"].ToString())));
                        }
                        catch
                        {
                            Console.WriteLine("Wrong link");
                        }
                }
                Send("Вибирайте те що до смаку)", m.Chat.Id, Menu.Order(Users[m.Chat.Id].order.Count + Users[m.Chat.Id].orderCreatePizza.Count));
                Users[m.Chat.Id].Status = UserStat.EditOrder;
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            DB.Close();
        }
        private static void ChoosingGropup(Message m)
        {
            switch (m.Text)
            {
                case "Піци":
                    SendDeskrip(m, GroupDish.Pizza);
                    break;
                case "Напої":
                    SendDeskrip(m, GroupDish.Drinks);
                    break;
                case "Своя піца":
                    Send("В наші піцерії є можливість створити власну піцу", m.Chat.Id);
                    break;
                case "Інші страви":
                    SendDeskrip(m, GroupDish.Other);
                    break;
                case "Назад":
                    Users[m.Chat.Id].Status = UserStat.WaitCommand;
                    Send("Чим можу допомогти?", m.Chat.Id, Menu.main_menu);
                    break;
                default:
                    Send("Що?", m.Chat.Id);
                    break;
            }
        }
        private async static void SendDeskrip(Message m, GroupDish group)
        {
            DB.OpenAsync();
            while (DB.GetConnection().State == ConnectionState.Executing) ;
            SqlCommand command;
            if (group == GroupDish.Pizza)
                command = new SqlCommand("SELECT * FROM[Dishes] WHERE type = 'Pizza'", DB.GetConnection());
            else if (group == GroupDish.Drinks)
                command = new SqlCommand("SELECT * FROM[Dishes] WHERE type = 'Drinks'", DB.GetConnection());
            else
                command = new SqlCommand("SELECT * FROM[Dishes] WHERE type = 'Other'", DB.GetConnection());
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                int counter = 0;
                while (await reader.ReadAsync())
                {
                    counter++;
                    if (reader["LinkIMG"].ToString() == "")
                    {
                        string text = String.Format($"{counter}. {reader["Name"]} \n Ціна: {reader["prise"]} грн \n {reader["Deskription"]}");
                        Send(text, m.Chat.Id);
                    }
                    else
                        try
                        {
                            string text = String.Format($"{counter}. {reader["Name"]} \n Ціна: {reader["prise"]} грн \n {reader["Deskription"]}");
                            SendPhoto(reader["LinkIMG"].ToString(), m.Chat.Id, text);
                        }
                        catch
                        {
                            Console.WriteLine("Wrong link");
                        }
                }
                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            DB.Close();
        }
        static async void SendToAdmin(Message m)
        {
            StreamWriter sw = new StreamWriter("Propose.txt");
            Console.WriteLine("Користувач {1} передає {0}", m.Text, m.Chat.FirstName + " " + m.Chat.LastName);
            await sw.WriteLineAsync("[{DateTime.Now.Day}:{DateTime.Now.Month}:{DateTime.Now.Year}]" +
                $" [{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}] Name:{m.Chat.FirstName} Text: {m.Text}");
            Send("Обовязково передамо адміністрації", m.Chat.Id, Menu.main_menu);
            Users[m.Chat.Id].Status = UserStat.WaitCommand;
            sw.Close();
        }
        static async void Send(string Text, long id, IReplyMarkup r = null)
        {
            try
            {
                await bot.SendTextMessageAsync(text: Text, chatId: id, replyMarkup: r);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException)
            {
                Console.WriteLine("Помилка відправлення");
            }
            DateTime stop = DateTime.Now;

            Console.WriteLine($"Відправлено \n {@Text} \n затрачено : {(stop - Users[id].start).Milliseconds} мс");
        }
        static async void SendPhoto(string link, long id, string Text, IReplyMarkup r = null)
        {
            try
            {
                await bot.SendPhotoAsync(id, link, Text, replyMarkup: r);
                DateTime stop = DateTime.Now;
                Console.WriteLine($"Відправлено \n {@Text} \n затрачено : {(stop - Users[id].start).Milliseconds} мс");
            }
            catch
            {
                Console.WriteLine("Error sending");
            }
        }
    }
}