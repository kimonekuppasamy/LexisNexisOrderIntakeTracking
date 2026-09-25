import { ChangeDetectorRef, Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { CustomerService } from './core/services/customer';
import { CartService } from './core/services/cart';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  constructor(private customerService: CustomerService, private changeDetectorRef: ChangeDetectorRef, private router: Router, private cartService: CartService) { }

  get isAdmin(): boolean {
    return this.customerService.getCurrentCustomer()?.isAdmin ?? false;
  }

  get isLoggedIn(): boolean {
    return this.customerService.getCurrentCustomer() != null ? true : false;
  }

  logout(): void {
    this.customerService.logout().subscribe(() => {
      this.customerService.setCurrentCustomer(null);
      this.cartService.clearCart();

      window.location.reload();
    });
  }
}