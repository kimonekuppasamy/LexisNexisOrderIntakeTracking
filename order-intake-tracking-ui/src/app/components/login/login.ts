import { Component,ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomerService } from '../../core/services/customer';
import { ActivatedRoute, Router } from '@angular/router';
import { Customer } from '../../models/Customer';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {

  email = '';
  customerName = '';
  phone = '';

  invalidCustomer:Boolean|null = null;
  errorMessage = '';
  constructor(
    private customerService: CustomerService,
    private router: Router,
    private route: ActivatedRoute,
    private changeDetectorRef: ChangeDetectorRef
  ) { }

  login(): void {
    this.errorMessage = '';

    this.customerService.getCustomerByEmail(this.email)
      .subscribe({
        next: customer => {

          if (customer === null) {
            this.errorMessage = 'Customer not found. Create customer account before proceeding.';
            this.invalidCustomer = true;
            this.customerService.setCurrentCustomer(null);
            this.changeDetectorRef.detectChanges();
            return;
          }
          this.customerService.setCurrentCustomer(customer);

          if (customer.isAdmin) {
            this.router.navigate(['/orders/admin']);
            return;
          }

          const returnUrl =
            this.route.snapshot.queryParamMap.get('returnUrl');

          if (returnUrl) {
            this.router.navigateByUrl(returnUrl);
          } else {
            this.router.navigate(['/orders']);
          }
        },

        error: error => {
          if (error.status === 404) {
            this.invalidCustomer = true;
            this.errorMessage =
              'Customer not found. Create customer account before proceeding.';

            this.changeDetectorRef.detectChanges();
          } else {
            this.errorMessage = 'Unable to login. Please try again.';
            this.changeDetectorRef.detectChanges();
          }
        }
      });
  }

createCustomer(): void {
  const customer: Customer = {
    customerId: crypto.randomUUID(),
    customerName: this.customerName,
    email: this.email,
    phone: this.phone,
    isAdmin: false
  };

  this.customerService.createCustomer(customer).subscribe({
    next: customer => {
      this.customerService.setCurrentCustomer(customer);
      this.router.navigate(['/cart']);
    },
    error: error => {
      this.errorMessage = 'Failed to create account.';
    }
  });
}
}