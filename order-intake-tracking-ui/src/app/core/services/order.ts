import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, OrderStatus } from '../../models/Order';
import { Cart } from '../../models/Cart';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  private readonly apiUrl = 'https://localhost:44368/api/';

  constructor(private http: HttpClient) { }

  getOrders(customerId: string, isAdmin: boolean): Observable<Order[]> {
    const params = new HttpParams()
      .set('customerId', customerId)

    if (isAdmin) {
      return this.http.get<Order[]>(this.apiUrl + "admin/orders", { params });
    }
    else {

      return this.http.get<Order[]>(this.apiUrl + "customer/orders", { params });
    }
  }

  submitOrder(order: Cart): Observable<Cart> {
    return this.http.post<Cart>(this.apiUrl + "customer/orders", order);
  }

  updateOrderStatus(orderId: string, status: OrderStatus): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}admin/orders/${orderId}?status=${status}`,
      null
    );
  }

  getOrder(orderNumber: string, customerId: string | null, isAdmin: boolean): Observable<Order> {

    const encodedOrderNumber = encodeURIComponent(orderNumber.trim());

    if (isAdmin) {
      return this.http.get<Order>(this.apiUrl + "admin/orders/" + encodedOrderNumber);
    }
    else {
      const params = new HttpParams()
        .set('customerId', customerId || '');
      return this.http.get<Order>(this.apiUrl + "customer/orders/" + encodedOrderNumber, { params });
    }
  }
}