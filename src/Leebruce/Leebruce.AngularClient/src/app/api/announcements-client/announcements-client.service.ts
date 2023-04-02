import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { tap } from 'rxjs';
import { AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT } from '../authentication/authentication.service';

@Injectable( /*{
  providedIn: 'root'
}*/ )
export class AnnouncementsClientService {

  constructor(
    private http: HttpClient ) { }

  public getAnnouncements() {
    let context = AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT
    return this.http.get<AnnouncementModel[]>( 'api/announcements', { context: context } )
      .pipe(
        tap( resp => resp.forEach( ann => ann.date = new Date( ann.date ) ) )
      );
  }

}

export interface AnnouncementModel {
  title: string,
  date: Date,
  author: string,
  content: string
}