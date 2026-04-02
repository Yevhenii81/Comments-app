import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommentService } from '../../services/comment.service';

@Component({
  selector: 'app-comment-form',
  templateUrl: './comment-form.component.html',
  standalone: false
})

export class CommentFormComponent implements OnInit {
  @Input() parentId: number | null = null;
  @Output() commentAdded = new EventEmitter<void>();

  form!: FormGroup;
  captchaToken = '';
  captchaImage = '';
  selectedFile: File | null = null;
  fileError = '';
  previewMode = false;
  loading = false;
  errorMessage = '';

  constructor(private fb: FormBuilder, private commentService: CommentService) {}

  ngOnInit() {
    this.form = this.fb.group({
      userName: ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9]+$/)]],
      email: ['', [Validators.required, Validators.email]],
      homePage: ['', [Validators.pattern(/^https?:\/\/.+/)]],
      captchaAnswer: ['', Validators.required],
      text: ['', [Validators.required, Validators.maxLength(5000)]]
    });
    this.loadCaptcha();
  }

  loadCaptcha() {
    this.commentService.getCaptcha().subscribe(res => {
      this.captchaToken = res.token;
      this.captchaImage = 'data:image/png;base64,' + res.imageBase64;
    });
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    this.fileError = '';
    this.selectedFile = null;
    if (!file) return;

    const ext = file.name.split('.').pop()?.toLowerCase();
    const allowedImages = ['jpg', 'jpeg', 'gif', 'png'];

    if (ext === 'txt' && file.size > 100 * 1024) {
      this.fileError = 'Текстовый файл не должен превышать 100 КБ';
      return;
    }
    if (!allowedImages.includes(ext ?? '') && ext !== 'txt') {
      this.fileError = 'Разрешены только JPG, GIF, PNG, TXT';
      return;
    }
    this.selectedFile = file;
  }

  wrapSelection(tag: string) {
    const textarea = document.getElementById('messageText') as HTMLTextAreaElement;
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    if (start === end) {
      return; 
    }

    const current = this.form.get('text')!.value;
    const selected = current.substring(start, end);

    const markers: Record<string, string> = {
      strong: '**',
      i: '*',
      code: '`'
    };
    const marker = markers[tag] ?? '';

    let wrapped = selected;
    if (selected.startsWith(marker) && selected.endsWith(marker) && selected.length > marker.length * 2) {
      wrapped = selected.substring(marker.length, selected.length - marker.length);
    } else {
      wrapped = `${marker}${selected}${marker}`;
    }

    const updated = current.substring(0, start) + wrapped + current.substring(end);
    this.form.get('text')!.setValue(updated);
  }

  get formattedText(): string {
    const text = this.form.get('text')?.value ?? '';
    const escaped = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    return escaped
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.+?)\*/g, '<em>$1</em>')
      .replace(/`(.+?)`/g, '<code>$1</code>');
  }

  insertLink() {
    const href = prompt('Введи URL (https://...)');
    if (!href) return;
    const title = prompt('Введи title (необязательно)') || '';
    const textarea = document.getElementById('messageText') as HTMLTextAreaElement;
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selected = textarea.value.substring(start, end) || 'ссылка';
    const titleAttr = title ? ` title="${title}"` : '';
    const tag = `<a href="${href}"${titleAttr}>${selected}</a>`;
    const current = this.form.get('text')!.value;
    this.form.get('text')!.setValue(
      current.substring(0, start) + tag + current.substring(end)
    );
  }

  formatCommentText(text: string): string {
    if (!text) {
      return '';
    }

    const escaped = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    return escaped
      .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.+?)\*/g, '<em>$1</em>')
      .replace(/`(.+?)`/g, '<code>$1</code>');
  }

  togglePreview() {
    this.previewMode = !this.previewMode;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.errorMessage = '';

    const fd = new FormData();
    fd.append('userName', this.form.value.userName);
    fd.append('email', this.form.value.email);
    if (this.form.value.homePage) fd.append('homePage', this.form.value.homePage);
    fd.append('captchaToken', this.captchaToken);
    fd.append('captchaAnswer', this.form.value.captchaAnswer);
    fd.append('text', this.form.value.text);
    if (this.parentId) fd.append('parentId', String(this.parentId));
    if (this.selectedFile) fd.append('file', this.selectedFile);

    this.commentService.createComment(fd).subscribe({
      next: () => {
        this.form.reset();
        this.selectedFile = null;
        this.loadCaptcha();
        this.loading = false;
        this.commentAdded.emit();
      },
      error: err => {
        this.errorMessage = err.error?.error ?? 'Произошла ошибка';
        this.loadCaptcha();
        this.loading = false;
      }
    });
  }
}