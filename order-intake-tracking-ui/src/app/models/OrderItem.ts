import { Product } from "./Product";

export interface OrderItem {
  orderItemId: string;
  productId: string;
  quantity: number;
  price: number;
  orderId: string;
  product: Product;
}