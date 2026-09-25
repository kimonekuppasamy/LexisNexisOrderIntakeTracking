import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../../models/Product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private readonly apiUrl = 'https://localhost:44368/api';

  constructor(private http: HttpClient) { }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/customer/products/`);
  }

  updateProduct(productId: string, quantity: number, price: number): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/admin/products/${productId}`,
      { quantity, price }
    );
  }
}