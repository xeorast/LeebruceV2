import { Component, Input, OnInit } from '@angular/core';
import { SubjectGradesViewModel } from '../grades.view-model';

@Component( {
  selector: 'app-grades-subject-item',
  templateUrl: './grades-subject-item.component.html',
  styleUrls: [
    './grades-subject-item.component.scss',
    './grades-subject-item.component.colors.scss'
  ]
} )
export class GradesSubjectItemComponent implements OnInit {

  @Input()
  subject?: SubjectGradesViewModel
  count?: number
  newCount?: number

  colorClass?: string

  constructor() { }

  ngOnInit(): void {
    if ( this.subject ) {
      this.colorClass = `tile-${GradesSubjectItemComponent.getSubjectColor( this.subject.subject )}`
      this.count = this.subject.firstTermGrades.length + this.subject.secondTermGrades.length
      this.newCount = this.subject.newGrades.firstTerm.length + this.subject.newGrades.secondTerm.length
    }
    this.colorClass ??= `tile-${GradesSubjectItemComponent.getRandomItem( GradesSubjectItemComponent.tileNumbers )}`
  }

  static tileNumbers = [0, 1, 2, 3, 4, 6, 8, 10, 14, 16, 18, 20, 22, 24, 26, 28, 32]
  static getRandomInt( min: number, max: number ) {
    min = Math.ceil( min );
    max = Math.floor( max );
    return Math.floor( Math.random() * ( max - min ) + min );
  }

  static getRandomItem( arr: any[] ) {
    return arr[GradesSubjectItemComponent.getRandomInt( 0, arr.length )]
  }

  static getKnownColor( subject: string ) {
    switch ( subject ) {
      case "Język angielski":
        return 12;
      case "Fizyka":
        return 7;
      case "Religia":
        return 0;
      case "Historia":
        return 18;
      case "Wychowanie fizyczne":
        return 30;
      default:
        return undefined
    }
  }

  static hash( str: string ) {
    let hash = 0
    let chr
    for ( let i = 0; i < str.length; i++ ) {
      chr = str.charCodeAt( i );
      hash = ( ( hash << 5 ) - hash ) + chr;
      hash |= 0; // Convert to 32bit integer
    }
    return hash;
  }

  static getSubjectColor( subject: string ) {
    let known = GradesSubjectItemComponent.getKnownColor( subject )
    if ( known != undefined ) {
      return known
    }

    let hash = Math.abs( GradesSubjectItemComponent.hash( subject ) )
    let hash2 = Math.floor( Math.sqrt( hash ) )
    let idx = Math.abs( hash2 ) % GradesSubjectItemComponent.tileNumbers.length
    let hue = GradesSubjectItemComponent.tileNumbers[idx]

    return hue
  }

}
