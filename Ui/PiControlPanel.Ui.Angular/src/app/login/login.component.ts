import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { IUserAccount } from '../shared/interfaces/userAccount';
import { AuthService } from '../shared/services/auth.service';

@Component({
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  userAccount: IUserAccount;

  constructor(private router: Router,
    private authService: AuthService) {
  }

  ngOnInit() {
    this.userAccount = {} as IUserAccount;
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }
  
  onSubmit() {
    this.authService.login(this.userAccount).subscribe(
      result => {
        if (result) {
          this.router.navigate(['/dashboard']);
        }
        else {
          alert('An error happened while trying to login');
        }
      },
      error => {
        alert(error);
      });
  }

  isLoggedIn() {
    return this.authService.isLoggedIn();
  }

}