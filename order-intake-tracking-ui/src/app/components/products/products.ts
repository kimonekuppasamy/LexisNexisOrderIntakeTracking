import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ProductService } from '../../core/services/product';
import { Product } from '../../models/Product';
import { Router } from '@angular/router';
import { CartService } from '../../core/services/cart';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../core/services/customer';
@Component({
  selector: 'app-products',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.css'
})

export class ProductsComponent implements OnInit {

  products: Product[] = [];
  loading = true;

  constructor(
    private productService: ProductService,
    private router: Router,
    private cartService: CartService,
    private changeDetectorRef: ChangeDetectorRef,
    private customerService: CustomerService
  ) { }

  ngOnInit(): void {
    this.loadProducts();
  }
  get isAdmin(): boolean {
    return this.customerService.getCurrentCustomer()?.isAdmin ?? false;
  }
  loadProducts(): void {

    this.productService.getProducts()
      .subscribe({
        next: products => {

          this.products = products;
          this.loading = false;

          this.changeDetectorRef.detectChanges();
        },

        error: error => {
          console.error('Product API error:', error);
          this.loading = false;
        }
      });
  }

  addToCart(product: Product): void {
    this.cartService.addToCart(product);
  }

  totalItemsInCart(): number {
    return this.cartService.getTotalItems();
  }
  viewCart(): void {
    this.router.navigate(['/cart']);
  }

  editingProductId: string | null = null;

  editProduct(product: Product): void {
    this.editingProductId = product.productId;
  }

  cancelEdit(): void {
    this.editingProductId = null;
  }

  saveProduct(productId: string, quantity: number, price: number): void {
    this.productService.updateProduct(productId, quantity, price).subscribe({
      next: () => {
        this.cancelEdit();
        this.loadProducts();
      }
    });
  }
}