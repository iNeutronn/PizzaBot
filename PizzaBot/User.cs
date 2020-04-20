using System;
using System.Collections.Generic;

namespace PizzaBot
{
    class User
    {
        public UserStat Status;
        public List<int> order;
        public DateTime start;
        public PizzaStat PizzaStat;
        public List<string> orderCreatePizza;
        public string currentpizza;
        public List<string> addons;
        
        public User()
        {
            addons = new List<string>();
            order = new List<int>();
            orderCreatePizza = new List<string>();
            Status = UserStat.WaitCommand;
            start = new DateTime();
        }
        public void Clear_card()
        {
            addons = new List<string>();
            order = new List<int>();
            orderCreatePizza = new List<string>();
            Status = UserStat.WaitCommand;
        }
    }
}