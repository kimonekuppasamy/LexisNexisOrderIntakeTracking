import { OrderItem } from './OrderItem';

export interface Order {
  orderId: string;
  orderNumber: string;
  customerId: string;
  orderTotal: number;
  orderStatus: OrderStatus;
  orderDate: string;
  orderCompletedDate: string | null;
  orderItems: OrderItem[] | null;
  notes: string;
  customerName: string | null;
}

export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Shipped = 2,
  Delivered = 3,
  Cancelled = 4
}