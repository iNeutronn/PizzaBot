using Telegram.Bot.Types.ReplyMarkups;

namespace PizzaBot
{
    class Menu
    {
        public static readonly ReplyKeyboardMarkup main_menu = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
               {
                    new[]
                    {
                             new KeyboardButton("Замовити щось"),
                             new KeyboardButton("Ознайомитись з меню")
                    },
                    new[]
                    {
                        new KeyboardButton("Надіслати повідомлення адміністрації"),
                        new KeyboardButton("Змінити інформацію"),
                        new KeyboardButton("Наші контакти")
                    }

                },
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        public static readonly InlineKeyboardMarkup back = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Назад"));
        public static readonly ReplyKeyboardMarkup dish_type = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
                 new[]
                    {
                             new KeyboardButton("Піци"),
                             new KeyboardButton("Своя піца"),
                             new KeyboardButton("Напої")
                    },
                    new[]
                    {
                        new KeyboardButton("Інші страви"),
                        new KeyboardButton("Назад")
                    }
            },
            ResizeKeyboard = true
        };
        public static InlineKeyboardMarkup AddToAhopCart(int id)
        {
            string callback = "AddToShopCard " + id;
            return new InlineKeyboardMarkup(
                 InlineKeyboardButton.WithCallbackData("Додати до корзини", callback)
                );
        }
        public static ReplyKeyboardMarkup Order(int count)
        {
            string t;
            if (count == 0)
                t = "Виберайте те що досмаку)";
            else
                t = $"Оформити замовлення ({count})";
            return new ReplyKeyboardMarkup
            {
                Keyboard = new[]
                {
                    new[]
                    {
                        new KeyboardButton(t),
                        new KeyboardButton("Назад")
                    },
                    new[]
                    {
                        new KeyboardButton("Очистити кошик")
                    }
                },
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        }
        public static ReplyKeyboardMarkup ConfigmNo = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
                new []
                {
                    new KeyboardButton("Так підтвердити замовлення"),
                    new KeyboardButton("Ні відмінити")
                }
            },
            ResizeKeyboard = true
        };
        public static readonly ReplyKeyboardMarkup ChangeInf = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
                new[]
                {
                    new KeyboardButton("Змінити номер телефону"),
                    new KeyboardButton("Змінити адрес доставки")
                },
                new[]
                {
                    new KeyboardButton("Назад")
                }
            },
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
        public static readonly ReplyKeyboardMarkup Size = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
                new []
                {
                    new KeyboardButton("XL"),
                    new KeyboardButton("L")
                },
                new []
                {
                    new KeyboardButton("M"),
                    new KeyboardButton("S")
                }
            },
            ResizeKeyboard = true
        };
        public static readonly ReplyKeyboardMarkup Sauce = new ReplyKeyboardMarkup
        {
            Keyboard =  new []
            {
                new []
                {
                    new KeyboardButton("Томатний"),
                    new KeyboardButton("Гострий")
                },
                new []
                {
                    new KeyboardButton("Білий")
                }
            },
            ResizeKeyboard= true
        };
        public static readonly ReplyKeyboardMarkup Cheese = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
                new[]
                {
                    new KeyboardButton("Російський"),
                    new KeyboardButton("Голандський")
                },
                new[]
                {
                    new KeyboardButton("Моцарелла")
                }
            },
            ResizeKeyboard = true
        };
        public static readonly ReplyKeyboardMarkup Additions = new ReplyKeyboardMarkup
        {
            Keyboard = new[]
            {
                new []
                {
                    new KeyboardButton("Додати піцу")
                },
                new []
                {
                    new KeyboardButton("Салямі"),
                    new KeyboardButton("Печериці")
                },
                new []
                {
                    new KeyboardButton("Томати"),
                    new KeyboardButton("Шампіньйони")
                }
            }
        };
        public static readonly ReplyKeyboardMarkup ConfigCreate = new ReplyKeyboardMarkup 
        {
            Keyboard = new[]
            {
                new []
                {
                    new KeyboardButton("Так оформити замовлення"),
                    new KeyboardButton("Ні продовжити покупки")
                }
            },
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };
    }
}