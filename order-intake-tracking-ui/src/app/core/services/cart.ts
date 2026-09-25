import { Injectable } from '@angular/core';
import { Cart, CartProduct } from '../../models/Cart';
import { Product } from '../../models/Product';

@Injectable({
  providedIn: 'root'
})
export class CartService {

  private cart: Cart = {
    cartId: crypto.randomUUID(),
    customerId: null,
    cartProducts: []
  };

  getCart(): Cart {
    return this.cart;
  }

  addToCart(product: Product): void {

    const existingProduct = this.cart.cartProducts.find(
      x => x.productId === product.productId
    );

    if (existingProduct) {

      if (existingProduct.quantity < product.productQuantity) {
        existingProduct.quantity++;
      }

    } else {

      const cartProduct: CartProduct = {
        productId: product.productId,
        quantity: 1,
        price: product.productPrice,
        product: product
      };

      this.cart.cartProducts.push(cartProduct);
    }
  }

  increaseQuantity(item: CartProduct): void {

    if (item.product) {

      if (item.quantity < item.product.productQuantity) {
        item.quantity++;
      }

    }
  }

  decreaseQuantity(item: CartProduct): void {

    if (item.quantity > 1) {
      item.quantity--;
    }
  }

  removeItem(item: CartProduct): void {

    this.cart.cartProducts = this.cart.cartProducts.filter(
      x => x.productId !== item.productId
    );
  }

  getTotalItems(): number {

    return this.cart.cartProducts.reduce(
      (total, item) => total + item.quantity,
      0
    );
  }

  getCartTotal(): number {

    return this.cart.cartProducts.reduce(
      (total, item) => total + (item.price * item.quantity),
      0
    );
  }

  // keep cart prices and stock in line with the current product data
  refreshProducts(products: Product[]): void {
    for (const item of this.cart.cartProducts) {
      const product = products.find(x => x.productId === item.productId);
      if (product) {
        item.price = product.productPrice;
        item.product = product;
      }
    }
  }

  clearCart(): Cart {
    this.cart = {
      cartId: crypto.randomUUID(),
      customerId: null,
      cartProducts: []
    }
    return this.cart;
  }
}