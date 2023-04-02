import { Component, OnInit } from '@angular/core';
import { Modal } from 'bootstrap';
import { Subscription } from 'rxjs';
import { AttachmentModel, MessageMetadataModel, MessageModel, MessagesClientService } from '../api/messages-client/messages-client.service';

@Component( {
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
} )
export class MessagesComponent implements OnInit {

  constructor(
    private messagesService: MessagesClientService
  ) { }

  public modalTitle?: string
  public modalBody?: string

  public messages?: MessageMetadataModel[]
  public currentMessageId?: string
  public currentMessage?: MessageModel
  public fetchingMessageId?: string
  public fetchingAttachmentIds: string[] = []

  public totalPages: number = 3
  public currentPage: number = 1

  private msgLoadingSub?: Subscription
  private modalObj?: Modal

  ngOnInit(): void {
    this.fetchMessages( 1 )

    let modalElem = document.getElementById( 'openMessageModal' )!

    modalElem.addEventListener( 'hide.bs.modal', _event => this.clearClosedMessage() )
    this.modalObj = new Modal( modalElem, {} )
  }

  fetchMessages( page: number ) {
    this.currentPage = page
    this.messages = undefined
    this.messagesService.getMessages( page ).subscribe( {
      next: res => {
        this.messages = res.elements
        this.totalPages = res.totalPages
        this.currentPage = res.currentPage
      }
    } )
  }

  fetchMessage( message: MessageMetadataModel ) {
    // do not reinit fetch of the same message
    if ( message.id == this.fetchingMessageId ) {
      return;
    }
    // cancel previous fetch if any
    this.clearClosedMessage()

    // init fetch
    this.fetchingMessageId = message.id
    this.currentMessageId = message.id
    this.msgLoadingSub = this.messagesService.getMessage( message.id ).subscribe( {
      next: res => {
        this.fetchingMessageId = undefined
        this.showMessage( res );
        message.isUnread = false
      }
    } )
  }

  showMessage( message: MessageModel ) {
    // set current message
    this.currentMessage = message

    // Update the modal's content.
    this.modalTitle! = this.currentMessage.subject
    this.modalBody! = this.currentMessage.content

    // show modal
    this.modalObj!.show()
  }

  clearClosedMessage() {
    // cancel previous fetch if any
    this.msgLoadingSub?.unsubscribe();
    this.msgLoadingSub = undefined;

    // clear message selection
    this.currentMessage = undefined
    this.currentMessageId = undefined

    // clear modal
    this.modalTitle = undefined
    this.modalBody = undefined
  }

  downloadAttachment( attachment: AttachmentModel ) {
    if ( this.isAttachmentPending( attachment.id ) ) {
      return
    }

    this.markAttachmentDownload( attachment.id )

    this.msgLoadingSub = this.messagesService.downloadAttachment( attachment ).subscribe( {
      complete: () => this.markAttachmentDownloadFinish( attachment.id ),
      error: async error => {
        this.markAttachmentDownloadFinish( attachment.id )
      }
    } )
  }

  isAttachmentPending( id: string ) {
    return this.fetchingAttachmentIds.includes( id )
  }
  markAttachmentDownload( id: string ) {
    this.fetchingAttachmentIds.push( id )
  }
  markAttachmentDownloadFinish( id: string ) {
    this.fetchingAttachmentIds.splice( this.fetchingAttachmentIds.indexOf( id ) )
  }


  getActiveClass( id: string ) {
    return id == this.currentMessageId ? 'active' : undefined
  }

  getAttachmentIconClass( fileName: string ) {
    const cls = 'bi-file-earmark'
    let extension = fileName.split( '.' ).at( -1 )?.toLowerCase()
    switch ( extension ) {
      case 'pdf':
        return cls + '-pdf'
      case 'zip':
        return cls + '-zip'
      case 'exe':
        return 'filetype-exe'
      case 'txt':
      case 'doc':
      case 'docx':
      case 'odt':
      case 'rtf':
        return cls + '-text'
      case 'xls':
      case 'xlsx':
      case 'ods':
      case 'odt':
      case 'rtf':
        return cls + '-spreadsheet'
      case 'ppt':
      case 'pptx':
      case 'odp':
        return cls + '-slides'
      case 'png':
      case 'jpg':
      case 'jpeg':
      case 'svg':
      case 'webp':
      case 'bmp':
      case 'gif':
        return cls + '-image'
      case 'mp4':
      case 'mov':
      case 'avi':
      case 'mkv':
      case 'webm':
        return cls + '-play'
      case 'mp3':
      case 'm4a':
      case 'wav':
      case 'aac':
        return cls + '-music'
      default: return cls
    }
  }

}
