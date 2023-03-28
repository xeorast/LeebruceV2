import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { catchError, map, Observable, tap } from 'rxjs';
import { AuthenticationService, NotAuthenticatedError } from '../authentication/authentication.service';
import { CollectionPage } from '../collection-page';
import { FILESAVER, FileSaver } from '../fileSaver';
import { HttpError, HttpProblem, ProblemDetails } from '../problem-details';

@Injectable()
export class MessagesClientService {

  constructor(
    private http: HttpClient,
    private auth: AuthenticationService,
    @Inject( FILESAVER ) private fileSaver: FileSaver ) { }

  public getMessages( page = 1 ) {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get<CollectionPage<MessageMetadataModel>>( `api/messages?page=${page}`, { headers: { Authorization: authHeader } } )
      .pipe(
        tap( resp => resp.elements.forEach( msg => msg.date = new Date( msg.date ) ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  public getMessage( id: string ) {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get<MessageModel>( `api/messages/${id}`, { headers: { Authorization: authHeader } } )
      .pipe(
        tap( resp => resp.date = new Date( resp.date ) ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  public downloadAttachment( attachment: AttachmentModel ) {
    let authHeader = `Bearer ${this.auth.token}`;
    return this.http.get( `api/messages/attachments/${attachment.id}`, { headers: { Authorization: authHeader }, responseType: 'blob' } )
      .pipe(
        tap( blob => this.fileSaver( blob, attachment.fileName ) ),
        map( _resp => { } ),
        catchError( error => this.errorHandler( error ) )
      );
  }

  private errorHandler( error: HttpError ): Observable<never> {
    let problemDetails: ProblemDetails | undefined
    if ( error instanceof HttpProblem ) {
      problemDetails = error.details
    }

    if ( error.response.status === 401 ) {
      throw new NotAuthenticatedError( problemDetails?.detail ?? undefined )
    }

    throw error
  }
}

export interface MessageMetadataModel {
  subject: string,
  author: string,
  date: Date,
  hasAttachments: boolean
  isUnread: boolean
  id: string
}

export interface MessageModel {
  subject: string,
  author: string,
  characterClass: string,
  date: Date,
  content: string,
  attachments: AttachmentModel[]
}

export interface AttachmentModel {
  fileName: string,
  id: string,
}
