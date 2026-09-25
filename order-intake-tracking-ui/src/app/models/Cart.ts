import { Product } from './Product';

export interface Cart {
  cartId: string;
  customerId: string | null;
  cartProducts: CartProduct[];
}

export interface CartProduct {
  productId: string;
  quantity: number;
  price: number;
  product: Product | null;
}