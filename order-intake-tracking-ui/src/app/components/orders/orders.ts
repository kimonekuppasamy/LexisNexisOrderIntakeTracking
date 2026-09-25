import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { OrderService } from '../../core/services/order';
import { CustomerService } from '../../core/services/customer';
import { Order, OrderStatus } from '../../models/Order';
import { Customer } from '../../models/Customer';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-orders',
  standalone: true,
  templateUrl: './orders.html',
  styleUrl: './orders.css',
  imports: [FormsModule]

})
export class OrdersComponent implements OnInit {

  orders: Order[] = [];
  orderNumber: string = '';
  loading = true;
  customer: Customer | null = null;
  displayedColumns: string[] = [
    'orderNumber',
    'items',
    'total',
    'status'
  ];

  successMessage: string | null = null;
  errorMessage: string | null = null;
  // mirrors AllowedTransitions in the API
  private allowedTransitions: Record<OrderStatus, OrderStatus[]> = {
    [OrderStatus.Pending]: [OrderStatus.Confirmed, OrderStatus.Cancelled],
    [OrderStatus.Confirmed]: [OrderStatus.Shipped, OrderStatus.Cancelled],
    [OrderStatus.Shipped]: [OrderStatus.Delivered],
    [OrderStatus.Delivered]: [],
    [OrderStatus.Cancelled]: []
  };

  statusOptions(order: Order): OrderStatus[] {
    return [order.orderStatus, ...this.allowedTransitions[order.orderStatus]];
  }
  constructor(
    private customerService: CustomerService,
    private orderService: OrderService,
    private changeDetectorRef: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.customer = this.customerService.getCurrentCustomer();
    console.log('Current customer:', this.customer);
    this.loadOrders();
  }

  loadOrders(): void {

    if (!this.customer) {
      this.loading = false;
      return;
    }

    this.orderService.getOrders(this.customer.customerId, this.customer.isAdmin)
      .subscribe({
        next: orders => {
          this.orders = orders;
          this.loading = false;
          this.changeDetectorRef.detectChanges();
        },

        error: error => {

          console.error('Failed to load orders:', error);

          this.loading = false;
        }
      });
  }
  getOrderStatus(status: OrderStatus): string {
    return OrderStatus[status];
  }

  updateOrderStatus(order: Order, event: Event): void {
    const select = event.target as HTMLSelectElement;
    const status = Number(select.value) as OrderStatus;

    this.successMessage = null;
    this.errorMessage = null;

    this.orderService.updateOrderStatus(order.orderId, status)
      .subscribe({
        next: () => {
          order.orderStatus = status;
          this.successMessage = 'Order status updated successfully.';
          this.changeDetectorRef.detectChanges();
        },
        error: (error) => {
          // the API returns the reason as a plain string
          this.errorMessage = typeof error.error === 'string' ? error.error : 'Failed to update order status.';
          select.value = String(order.orderStatus);
          this.changeDetectorRef.detectChanges();
        }
      });
  }

  getOrder(): void {

    if (!this.customer) {
      this.loading = false;
      return;
    }

    if (this.orderNumber.trim() === '') {
      this.loadOrders();
      return;
    }


    if (this.customer.isAdmin) {
      this.orderService.getOrder(this.orderNumber, null, this.customer.isAdmin)
        .subscribe({
          next: order => {
            this.orders = [order];
            this.loading = false;
            this.changeDetectorRef.detectChanges();
          },

          error: error => {

            console.error('Failed to load orders:', error);

            this.loading = false;
          }
        });
    }
    else {
      this.orderService.getOrder(this.orderNumber, this.customer.customerId, this.customer.isAdmin)
        .subscribe({
          next: order => {
            this.orders = [order];
            this.loading = false;
            this.changeDetectorRef.detectChanges();
          },

          error: error => {

            console.error('Failed to load orders:', error);

            this.loading = false;
          }
        });
    }


  }
}