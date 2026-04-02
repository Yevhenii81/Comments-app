import { Component } from '@angular/core';

@Component({
  selector: 'app-lightbox',
  templateUrl: './lightbox.component.html',
  styleUrls: ['./lightbox.component.css'],
  standalone: false
})

export class LightboxComponent {
  isOpen = false;
  imageUrl = '';

  open(url: string) {
    this.imageUrl = url;
    this.isOpen = true;
  }

  close() {
    this.isOpen = false;
  }
}