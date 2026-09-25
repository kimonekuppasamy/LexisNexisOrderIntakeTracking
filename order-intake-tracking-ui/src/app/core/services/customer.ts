import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { Customer } from '../../models/Customer';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {

  private readonly apiUrl =
    'https://localhost:44368/api/customer';

  private currentCustomerSubject =
    new BehaviorSubject<Customer | null>(null);

  currentCustomer$ =
    this.currentCustomerSubject.asObservable();

  constructor(private http: HttpClient) {}

  getCustomerByEmail(email: string): Observable<Customer> {

    const params = new HttpParams()
      .set('email', email);

    return this.http.get<Customer>(
      this.apiUrl,
      { params }
    );
  }

  setCurrentCustomer(customer: Customer | null): void {
    this.currentCustomerSubject.next(customer);
  }

  getCurrentCustomer(): Customer | null {
    return this.currentCustomerSubject.value;
  }

logout(): Observable<void> {
  return this.http.post<void>(
    `${this.apiUrl}/logout`,
    {}
  );
}
createCustomer(customer: Customer): Observable<Customer> {
    return this.http.post<Customer>(this.apiUrl, customer);
  }
}