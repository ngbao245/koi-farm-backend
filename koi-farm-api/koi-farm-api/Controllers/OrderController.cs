﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Data.Entity;
using Repository.Model;
using Repository.Model.Order;
using Repository.Repository;
using System.Linq;

namespace koi_farm_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly UnitOfWork _unitOfWork;
        public OrderController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private string GetUserIdFromClaims()
        {
            return User.FindFirst("UserID")?.Value;
        }

        //Create Order Endpoint
        [HttpPost("create")]
        public IActionResult CreateOrder([FromBody] CreateOrderRequestModel model)
        {
            var cart = _unitOfWork.CartRepository.GetSingle(c => c.Id == model.CartId, c => c.Items);
            if (cart == null || !cart.Items.Any())
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = 400,
                    MessageError = "There is no Cart found or the cart is empty."
                });
            }

            var order = new Order
            {
                UserId = GetUserIdFromClaims(),
                Total = 0,
                Status = "Pending",
                Items = new List<OrderItem>()
            };

            foreach (var cartItem in cart.Items)
            {
                var productItem = _unitOfWork.ProductItemRepository.GetById(cartItem.ProductItemId);
                if (productItem == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = 400,
                        MessageError = $"Product item with ID {cartItem.ProductItemId} not found."
                    });
                }

                if (cartItem.Quantity > productItem.Quantity)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = 400,
                        MessageError = $"Requested quantity for {productItem.Name} exceeds available stock."
                    });
                }

                var orderItem = new OrderItem
                {
                    ProductItemId = cartItem.ProductItemId,
                    Quantity = cartItem.Quantity
                };

                order.Items.Add(orderItem);
                order.Total += orderItem.Quantity * productItem.Price;

                if (orderItem.Quantity > productItem.Quantity)
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        MessageError = "Exceeds Quantity of Product Item"
                    });
                }

                productItem.Quantity -= orderItem.Quantity;
                
                _unitOfWork.ProductItemRepository.Update(productItem);
            }

            _unitOfWork.OrderRepository.Create(order);

            foreach (var cartItem in cart.Items.ToList())
            {
                _unitOfWork.CartItemRepository.Delete(cartItem);
            }

            _unitOfWork.CartRepository.Delete(cart);
            _unitOfWork.SaveChange();

            return Ok(new ResponseModel
            {
                StatusCode = 201,
                Data = new OrderResponseModel
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    Status = order.Status,
                    UserId = order.UserId,
                    Items = order.Items.Select(item => new OrderItemResponseModel
                    {
                        ProductItemId = item.ProductItemId,
                        Quantity = item.Quantity,
                        Price = _unitOfWork.ProductItemRepository.GetById(item.ProductItemId).Price
                    }).ToList()
                }
            });
        }

        //Get Order by ID Endpoint
        [HttpGet("{orderId}")]
        public IActionResult GetOrderById(string orderId)
        {
            var order = _unitOfWork.OrderRepository.GetSingle(o => o.Id == orderId, o => o.Items);
            if (order == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = 404,
                    MessageError = "Order not found."
                });
            }

            return Ok(new ResponseModel
            {
                StatusCode = 200,
                Data = new OrderResponseModel
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    Status = order.Status,
                    UserId = order.UserId,
                    Items = order.Items.Select(item => new OrderItemResponseModel
                    {
                        ProductItemId = item.ProductItemId,
                        Quantity = item.Quantity,
                        Price = _unitOfWork.ProductItemRepository.GetById(item.ProductItemId).Price
                    }).ToList()
                }
            });
        }

        //Get All Orders for a User Endpoint
        [HttpGet("user")]
        public IActionResult GetAllOrdersForUser()
        {
            var userId = GetUserIdFromClaims();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ResponseModel
                {
                    StatusCode = 401,
                    MessageError = "Unauthorized. User ID not found in claims."
                });
            }

            var orders = _unitOfWork.OrderRepository.Get(o => o.UserId == userId, o => o.Items).ToList();
            if (!orders.Any())
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = 404,
                    MessageError = "No orders found for the user."
                });
            }

            return Ok(new ResponseModel
            {
                StatusCode = 200,
                Data = orders.Select(order => new OrderResponseModel
                {
                    OrderId = order.Id,
                    Total = order.Total,
                    Status = order.Status,
                    UserId = order.UserId,
                    Items = order.Items.Select(item => new OrderItemResponseModel
                    {
                        ProductItemId = item.ProductItemId,
                        Quantity = item.Quantity,
                        Price = _unitOfWork.ProductItemRepository.GetById(item.ProductItemId).Price
                    }).ToList()
                }).ToList()
            });
        }
    }
}
