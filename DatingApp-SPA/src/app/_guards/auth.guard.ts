import { Injectable } from '@angular/core';
import { CanActivate, Router} from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/Alertify.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private authservice: AuthService,
    private router: Router,
    private alertifyservice: AlertifyService) {}

  canActivate(): Observable<boolean> | Promise<boolean> | boolean {
    if (this.authservice.loggedIn()){
      return true;
    }

    this.alertifyservice.warning('You shall not pass!!!');
    this.router.navigate(['/home']);
    return false;
  }
}
