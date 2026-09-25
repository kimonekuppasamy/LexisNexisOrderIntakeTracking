import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Cart, CartProduct } from '../../models/Cart';
import { CartService } from '../../core/services/cart';
import { OrderService } from '../../core/services/order';
import { CustomerService } from '../../core/services/customer';
import { Router } from '@angular/router';
import { ProductService } from '../../core/services/product';
@Component({
  selector: 'app-cart',
  standalone: true,
  templateUrl: './cart.html',
  styleUrl: './cart.css'
})
export class CartComponent implements OnInit {

  cart!: Cart;
  errorMessage: string | null = null;

  constructor(private cartService: CartService, private orderService: OrderService, private customerService: CustomerService, private router: Router,private changeDetectorRef: ChangeDetectorRef, private productService: ProductService) { }

  ngOnInit(): void {
    this.cart = this.cartService.getCart();
    this.refreshPrices();
  }

  refreshPrices(): void {
    this.productService.getProducts().subscribe(products => {
      this.cartService.refreshProducts(products);
      this.changeDetectorRef.detectChanges();
    });
  }

  increaseQuantity(item: CartProduct): void {
    this.cartService.increaseQuantity(item);
  }

  decreaseQuantity(item: CartProduct): void {
    this.cartService.decreaseQuantity(item);
  }

  removeItem(item: CartProduct): void {
    this.cartService.removeItem(item);
  }

  getTotalItems(): number {
    return this.cartService.getTotalItems();
  }

  getCartTotal(): number {
    return this.cartService.getCartTotal();
  }

  submitOrder(cart: Cart): void {
    if (!cart.cartId) {
      cart.cartId = crypto.randomUUID();
    }

    const customer = this.customerService.getCurrentCustomer();
    if (!customer) {
      this.router.navigate(['/login'], {
        queryParams: {
          returnUrl: '/cart'
        }
      });

      return;
    }
    else {
      cart.customerId = customer.customerId;
      this.orderService.submitOrder(cart).subscribe({
        next: (submittedOrder) => {
          this.cartService.clearCart();
          this.cart = this.cartService.getCart();
          
           this.changeDetectorRef.detectChanges();
           this.router.navigate(['/orders']);
        },
        error: (error) => {
          console.error('Failed to submit order:', error);
          this.errorMessage = typeof error.error === 'string' ? error.error : 'Failed to submit order.';
          this.refreshPrices();
        }
      });
    }

  }
 continueShopping(): void {
    this.router.navigate(['/products']);
  }

  
}