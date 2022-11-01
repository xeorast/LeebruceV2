import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { HttpError } from '../problem-details';

@Injectable( {
  providedIn: 'root'
} )
export class ErrorNotifierService {
  private errorsSubject: ReplaySubject<ServerError>
  public errors: Observable<ServerError>

  constructor() {
    this.errorsSubject = new ReplaySubject<ServerError>()
    this.errors = this.errorsSubject.asObservable()
  }

  raiseError( error: ServerError ) {
    this.errorsSubject.next( error )
  }
}

export class ServerError extends HttpError {
}

export class ProxyError extends ServerError {
}

export class TechnicalBreakError extends ProxyError {
}

export class ProxyTimeoutError extends ProxyError {
}

export class ServerConnectionError extends ServerError {
}
