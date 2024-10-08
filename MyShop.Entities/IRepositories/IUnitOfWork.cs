﻿using MyShop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Entities.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<Category> Categories { get; }
        IBaseRepository<Product> Products { get; }
        //IBaseRepository<ShoppingCart> ShoppingCarts { get; }
        IShoppingCartRepository ShoppingCarts { get; }
        IOrderDetailsRepository  OrderDetails { get; }
		IOrderHeaderRepository  OrderHeaders { get; }
        IUserRepository  userRepository { get; }

        int Complete();
    }
}
