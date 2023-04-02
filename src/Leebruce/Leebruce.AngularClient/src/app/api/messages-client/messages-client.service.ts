import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { map, tap } from 'rxjs';
import { AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT } from '../authentication/authentication.service';
import { CollectionPage } from '../collection-page';
import { FILESAVER, FileSaver } from '../fileSaver';

@Injectable()
export class MessagesClientService {

  constructor(
    private http: HttpClient,
    @Inject( FILESAVER ) private fileSaver: FileSaver ) { }

  public getMessages( page = 1 ) {
    let context = AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT
    return this.http.get<CollectionPage<MessageMetadataModel>>( `api/messages?page=${page}`, { context: context } )
      .pipe(
        tap( resp => resp.elements.forEach( msg => msg.date = new Date( msg.date ) ) )
      );
  }

  public getMessage( id: string ) {
    let context = AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT
    return this.http.get<MessageModel>( `api/messages/${id}`, { context: context } )
      .pipe(
        tap( resp => resp.date = new Date( resp.date ) )
      );
  }

  public downloadAttachment( attachment: AttachmentModel ) {
    let context = AUTH_ENABLED_REDIRECT_TO_LOGIN_CONTEXT
    return this.http.get( `api/messages/attachments/${attachment.id}`, { context: context, responseType: 'blob' } )
      .pipe(
        tap( blob => this.fileSaver( blob, attachment.fileName ) ),
        map( _resp => { } )
      );
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
