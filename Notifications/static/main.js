let container = document.querySelector('.container');

function createNotification (type, text, delay, duration) {

  let notificationEl = document.createElement('div');
  let iconEl = document.createElement('div');
  let textEl = document.createElement('div');
  let progressEl = document.createElement('div');

  iconEl.style.backgroundImage = `url(./static/${type.toLowerCase()}.png)`;
  textEl.innerText = text;

  iconEl.className = 'icon';
  textEl.className = 'text';
  progressEl.className = 'progress';

  notificationEl.appendChild(iconEl);
  notificationEl.appendChild(textEl);
  notificationEl.appendChild(progressEl);

  notificationEl.className = 'notification';

  container.appendChild(notificationEl);
  applyAppear(notificationEl, progressEl, delay, duration);
}

function applyAppear(notificationEl, progressEl, delay, duration) {
  notificationEl.classList.add('appear');
  setTimeout(() => {
    if (delay > 0) {
      applyDelay(notificationEl, progressEl, delay, duration-600);
    } else {
      applyActivation(notificationEl, progressEl, duration-600);
    }
    notificationEl.classList.remove('appear');
  }, 500);
}

function applyDelay(notificationEl, progressEl, delay, duration) {
  notificationEl.classList.add('delay');
  progressEl.style.animationDuration = `${delay/1000}s`;


  setTimeout(() => {
    notificationEl.classList.remove('delay');
    applyActivation(notificationEl, progressEl, duration);
  }, delay + 25);
}

function applyActivation(notificationEl, progressEl, duration) {
  notificationEl.classList.add('activate');

  if (duration > 0) {

    setTimeout(() => {
      notificationEl.classList.remove('activate');
      applyDuration(notificationEl, progressEl, duration - 750);
    }, 750)


  }
  else {
    setTimeout(() => {
      applyDisappear(notificationEl, progressEl);
      notificationEl.classList.remove('activate');
    }, 750)
  }
}

function applyDuration(notificationEl, progressEl, duration) {
  notificationEl.classList.add('duration');
  progressEl.style.animationDuration = `${duration / 1000}s`;

  setTimeout(() => {
    applyDisappear(notificationEl, progressEl);
  }, duration);
}

function applyDisappear(notificationEl, progressEl) {
  notificationEl.classList.add('disappear');
  setTimeout(() => {
    notificationEl.remove();
  }, 850);
}

function checkNewEvents() {

  fetch("http://127.0.0.1:51473/data/")
  .then(res => res.json())
  .then(res => {
    for (let i = 0; i < res.length; i++) {
      createNotification(res[i].eventType, res[i].text, res[i].delay, res[i].duration);
    }

    setTimeout(checkNewEvents, 750);
  })

}

checkNewEvents();