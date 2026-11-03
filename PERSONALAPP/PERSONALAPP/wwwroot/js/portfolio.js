(function () {
  "use strict";

  var nav = document.getElementById("pfNav");
  var navCollapse = document.getElementById("pfNavCollapse");
  var navToggle = document.getElementById("pfNavToggle");
  var scrollTopBtn = document.getElementById("pfScrollTop");
  var navLinks = document.querySelectorAll(".pf-nav-links a[href^='#']");
  var sections = document.querySelectorAll(".pf-section[id]");
  var navIndicator = document.getElementById("pfNavIndicator");
  var prefersReduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  function setNavScrolled() {
    if (!nav) return;
    if (window.scrollY > 24) {
      nav.classList.add("scrolled");
    } else {
      nav.classList.remove("scrolled");
    }
  }

  function setScrollTopVisible() {
    if (!scrollTopBtn) return;
    if (window.scrollY > 400) {
      scrollTopBtn.classList.add("visible");
    } else {
      scrollTopBtn.classList.remove("visible");
    }
  }

  function closeMobileNav() {
    if (navCollapse) {
      navCollapse.classList.remove("open");
    }
    if (navToggle) {
      navToggle.setAttribute("aria-expanded", "false");
    }
  }

  function updateNavIndicator(activeLink) {
    if (!navIndicator || !navCollapse || !activeLink) return;
    if (window.innerWidth < 992) {
      navIndicator.style.opacity = "0";
      return;
    }
    var collapseRect = navCollapse.getBoundingClientRect();
    var linkRect = activeLink.getBoundingClientRect();
    navIndicator.style.opacity = "1";
    navIndicator.style.width = linkRect.width + "px";
    navIndicator.style.left = linkRect.left - collapseRect.left + "px";
  }

  function setActiveNavLink() {
    var scrollPos = window.scrollY + 120;
    var currentId = "hero";
    var activeLink = null;

    sections.forEach(function (section) {
      var top = section.offsetTop;
      var height = section.offsetHeight;
      if (scrollPos >= top && scrollPos < top + height) {
        currentId = section.getAttribute("id") || "hero";
      }
    });

    navLinks.forEach(function (link) {
      var href = link.getAttribute("href");
      if (href === "#" + currentId) {
        link.classList.add("active");
        link.setAttribute("aria-current", "page");
        activeLink = link;
      } else {
        link.classList.remove("active");
        link.removeAttribute("aria-current");
      }
    });

    updateNavIndicator(activeLink);
  }

  function initReveal() {
    var reveals = document.querySelectorAll(".reveal");

    if (prefersReduced || !("IntersectionObserver" in window)) {
      reveals.forEach(function (el) {
        el.classList.add("visible");
      });
      return;
    }

    var observer = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            entry.target.classList.add("visible");
            observer.unobserve(entry.target);
          }
        });
      },
      { root: null, rootMargin: "0px 0px -8% 0px", threshold: 0.08 }
    );

    reveals.forEach(function (el) {
      observer.observe(el);
    });
  }

  function initCareerTimeline() {
    var milestones = document.querySelectorAll(".pf-career-milestone");
    if (!milestones.length) return;

    if (prefersReduced || !("IntersectionObserver" in window)) {
      milestones.forEach(function (m) {
        m.classList.add("visible");
      });
      return;
    }

    var observer = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (entry.isIntersecting) {
            var el = entry.target;
            var delay = parseInt(el.getAttribute("data-milestone") || "0", 10) * 120;
            setTimeout(function () {
              el.classList.add("visible");
            }, delay);
            observer.unobserve(el);
          }
        });
      },
      { root: null, threshold: 0.15 }
    );

    milestones.forEach(function (m) {
      observer.observe(m);
    });
  }

  function initTypewriter() {
    var el = document.getElementById("pfTypewriter");
    if (!el || prefersReduced) return;

    var roles = [
      "Software Engineer",
      "Full Stack Developer",
      "Backend Developer",
      ".NET Developer"
    ];
    var roleIndex = 0;
    var charIndex = 0;
    var deleting = false;

    function tick() {
      var current = roles[roleIndex];
      if (!deleting) {
        charIndex++;
        el.textContent = current.slice(0, charIndex);
        if (charIndex === current.length) {
          deleting = true;
          setTimeout(tick, 1800);
          return;
        }
        setTimeout(tick, 70);
      } else {
        charIndex--;
        el.textContent = current.slice(0, charIndex);
        if (charIndex === 0) {
          deleting = false;
          roleIndex = (roleIndex + 1) % roles.length;
          setTimeout(tick, 400);
          return;
        }
        setTimeout(tick, 35);
      }
    }

    tick();
  }

  function animateCounter(el, target, suffix, duration) {
    var start = 0;
    var startTime = null;

    function frame(timestamp) {
      if (!startTime) startTime = timestamp;
      var progress = Math.min((timestamp - startTime) / duration, 1);
      var value = Math.floor(start + (target - start) * progress);
      el.textContent = value + suffix;
      if (progress < 1) {
        requestAnimationFrame(frame);
      }
    }

    requestAnimationFrame(frame);
  }

  function initCounters() {
    var counters = document.querySelectorAll(".pf-stat-value[data-count]");
    if (!counters.length) return;

    if (prefersReduced || !("IntersectionObserver" in window)) {
      counters.forEach(function (el) {
        var target = parseInt(el.getAttribute("data-count"), 10);
        var suffix = el.getAttribute("data-suffix") || "";
        el.textContent = target + suffix;
      });
      return;
    }

    var observer = new IntersectionObserver(
      function (entries) {
        entries.forEach(function (entry) {
          if (!entry.isIntersecting) return;
          var el = entry.target;
          var target = parseInt(el.getAttribute("data-count"), 10);
          var suffix = el.getAttribute("data-suffix") || "";
          animateCounter(el, target, suffix, 1400);
          observer.unobserve(el);
        });
      },
      { threshold: 0.5 }
    );

    counters.forEach(function (el) {
      observer.observe(el);
    });
  }

  if (navToggle && navCollapse) {
    navToggle.addEventListener("click", function () {
      var isOpen = navCollapse.classList.toggle("open");
      navToggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
    });
  }

  navLinks.forEach(function (link) {
    link.addEventListener("click", function () {
      closeMobileNav();
    });
  });

  document.addEventListener("click", function (e) {
    if (!navCollapse || !navToggle) return;
    if (!navCollapse.classList.contains("open")) return;
    if (!nav.contains(e.target)) {
      closeMobileNav();
    }
  });

  if (scrollTopBtn) {
    scrollTopBtn.addEventListener("click", function () {
      window.scrollTo({ top: 0, behavior: prefersReduced ? "auto" : "smooth" });
    });
  }

  window.addEventListener("scroll", function () {
    setNavScrolled();
    setScrollTopVisible();
    setActiveNavLink();
  });

  window.addEventListener("resize", function () {
    setActiveNavLink();
  });

  setNavScrolled();
  setScrollTopVisible();
  setActiveNavLink();
  initReveal();
  initCareerTimeline();
  initTypewriter();
  initCounters();
})();
