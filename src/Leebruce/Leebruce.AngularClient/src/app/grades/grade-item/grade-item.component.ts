import { formatDate } from '@angular/common';
import { AfterViewInit, Component, ElementRef, HostBinding, Input, OnInit } from '@angular/core';
import { Tooltip } from 'bootstrap';
import { GradeModel, SpecialGrade } from 'src/app/api/grades-client/grades-client.service';

@Component( {
  selector: 'app-grade-item',
  templateUrl: './grade-item.component.html',
  styleUrls: ['./grade-item.component.scss'],
  host: {
    "data-bs-toggle": "tooltip",
    "data-bs-html": "true",
    "data-bs-placement": "bottom",
  }
} )
export class GradeItemComponent implements OnInit, AfterViewInit {

  @Input()
  grade?: GradeModel

  @HostBinding( "attr.data-bs-title" )
  bsTooltip = ""

  asTooltip?: Tooltip

  constructor(
    private elRef: ElementRef ) {
  }

  ngOnInit(): void {
    if ( this.grade ) {
      this.bsTooltip =
        `<h6 class="capitalise-first mb-0 text-inherit">${this.grade.category ?? ''}</h6>
        <small>${formatDate( this.grade.date, 'dd MMM yyyy', 'en-US' )}</small><br/>
        <p class="small align-start capitalise-first mb-0">${this.grade.comment ?? ''}</p>`
    }
  }

  ngAfterViewInit(): void {
    this.asTooltip = new Tooltip( this.elRef.nativeElement )
  }

  getShortSpecialValue( specialValue?: SpecialGrade ) {
    switch ( specialValue ) {
      case "Unprepared":
        return "Np"
      case "Plus":
        return "+"
      case "Minus":
        return "-"
      case undefined:
        return undefined
    }
  }

}
