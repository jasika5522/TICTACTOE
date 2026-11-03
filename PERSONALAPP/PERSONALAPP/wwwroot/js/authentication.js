(function () {
  "use strict";

  var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  var mobilePattern = /^[0-9]{10}$/;

  function showFeedback(input, message, isValid) {
    var field = input.closest(".auth-field");
    if (!field) return;
    var feedback = field.querySelector(".auth-feedback");
    if (!feedback) return;

    if (message) {
      feedback.textContent = message;
      feedback.classList.add("visible");
      input.classList.toggle("is-invalid", !isValid);
      input.classList.toggle("is-valid", isValid === true);
    } else {
      feedback.textContent = "";
      feedback.classList.remove("visible");
      input.classList.remove("is-invalid", "is-valid");
    }
  }

  function showAlert(form, type, message) {
    var alert = form.querySelector(".auth-alert");
    if (!alert) return;
    alert.className = "auth-alert visible auth-alert--" + type;
    alert.textContent = message;
  }

  function hideAlert(form) {
    var alert = form.querySelector(".auth-alert");
    if (!alert) return;
    alert.className = "auth-alert";
    alert.textContent = "";
  }

  function setLoading(button, loading) {
    if (!button) return;
    button.disabled = loading;
    button.classList.toggle("loading", loading);
  }

  function initPasswordToggles(root) {
    root.querySelectorAll("[data-toggle-password]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        var targetId = btn.getAttribute("data-toggle-password");
        var input = document.getElementById(targetId);
        if (!input) return;
        var isPassword = input.type === "password";
        input.type = isPassword ? "text" : "password";
        var icon = btn.querySelector("i");
        if (icon) {
          icon.className = isPassword ? "bi bi-eye-slash" : "bi bi-eye";
        }
        btn.setAttribute("aria-label", isPassword ? "Hide password" : "Show password");
      });
    });
  }

  function validateLogin(form) {
    hideAlert(form);
    var valid = true;
    var email = form.querySelector("#loginEmail");
    var password = form.querySelector("#loginPassword");

    if (!email.value.trim()) {
      showFeedback(email, "Email address is required.", false);
      valid = false;
    } else if (!emailPattern.test(email.value.trim())) {
      showFeedback(email, "Enter a valid email address.", false);
      valid = false;
    } else {
      showFeedback(email, "", null);
    }

    if (!password.value) {
      showFeedback(password, "Password is required.", false);
      valid = false;
    } else {
      showFeedback(password, "", null);
    }

    return valid;
  }

  function validateSignUp(form) {
    hideAlert(form);
    var valid = true;

    var fullName = form.querySelector("#signupFullName");
    var email = form.querySelector("#signupEmail");
    var mobile = form.querySelector("#signupMobile");
    var password = form.querySelector("#signupPassword");
    var confirmPassword = form.querySelector("#signupConfirmPassword");
    var terms = form.querySelector("#signupTerms");

    if (!fullName.value.trim()) {
      showFeedback(fullName, "Full name is required.", false);
      valid = false;
    } else {
      showFeedback(fullName, "", null);
    }

    if (!email.value.trim()) {
      showFeedback(email, "Email address is required.", false);
      valid = false;
    } else if (!emailPattern.test(email.value.trim())) {
      showFeedback(email, "Enter a valid email address.", false);
      valid = false;
    } else {
      showFeedback(email, "", null);
    }

    if (!mobile.value.trim()) {
      showFeedback(mobile, "Mobile number is required.", false);
      valid = false;
    } else if (!mobilePattern.test(mobile.value.trim())) {
      showFeedback(mobile, "Enter a valid 10-digit mobile number.", false);
      valid = false;
    } else {
      showFeedback(mobile, "", null);
    }

    if (!password.value) {
      showFeedback(password, "Password is required.", false);
      valid = false;
    } else if (password.value.length < 8) {
      showFeedback(password, "Password must be at least 8 characters.", false);
      valid = false;
    } else {
      showFeedback(password, "", null);
    }

    if (!confirmPassword.value) {
      showFeedback(confirmPassword, "Please confirm your password.", false);
      valid = false;
    } else if (confirmPassword.value !== password.value) {
      showFeedback(confirmPassword, "Passwords do not match.", false);
      valid = false;
    } else {
      showFeedback(confirmPassword, "", null);
    }

    if (!terms.checked) {
      showFeedback(terms, "You must accept the Terms & Conditions.", false);
      valid = false;
    } else {
      showFeedback(terms, "", null);
    }

    return valid;
  }

  function bindLiveValidation(form, type) {
    form.querySelectorAll("input").forEach(function (input) {
      input.addEventListener("input", function () {
        if (form.querySelector(".auth-alert.visible")) {
          hideAlert(form);
        }
        if (type === "login") {
          validateLogin(form);
        } else {
          validateSignUp(form);
        }
      });
      input.addEventListener("blur", function () {
        if (type === "login") {
          validateLogin(form);
        } else {
          validateSignUp(form);
        }
      });
    });
  }

  function initLoginForm() {
    var form = document.getElementById("loginForm");
    if (!form) return;

    initPasswordToggles(form);
    bindLiveValidation(form, "login");

    form.addEventListener("submit", function (e) {
      e.preventDefault();
      if (!validateLogin(form)) {
        showAlert(form, "error", "Please fix the highlighted fields and try again.");
        return;
      }

      var btn = form.querySelector('[type="submit"]');
      setLoading(btn, true);
      hideAlert(form);

      setTimeout(function () {
        setLoading(btn, false);
        showAlert(
          form,
          "success",
          "Demo only: form validation passed. Backend sign-in is not connected yet."
        );
      }, 1400);
    });
  }

  function initSignUpForm() {
    var form = document.getElementById("signupForm");
    if (!form) return;

    var mobile = form.querySelector("#signupMobile");
    if (mobile) {
      mobile.addEventListener("input", function () {
        mobile.value = mobile.value.replace(/\D/g, "").slice(0, 10);
      });
    }

    initPasswordToggles(form);
    bindLiveValidation(form, "signup");

    var isSubmitting = false;

    form.addEventListener("submit", function (e) {
      e.preventDefault();
      if (isSubmitting) return;

      if (!validateSignUp(form)) {
        showAlert(form, "error", "Please complete all required fields correctly.");
        return;
      }

      var registerUrl = form.getAttribute("data-register-url");
      var loginUrl = form.getAttribute("data-login-url");
      if (!registerUrl) {
        showAlert(form, "error", "Registration endpoint is not configured.");
        return;
      }

      var btn = form.querySelector('[type="submit"]');
      isSubmitting = true;
      setLoading(btn, true);
      hideAlert(form);

      var payload = {
        fullName: form.querySelector("#signupFullName").value.trim(),
        email: form.querySelector("#signupEmail").value.trim(),
        mobileNumber: form.querySelector("#signupMobile").value.trim(),
        password: form.querySelector("#signupPassword").value,
        confirmPassword: form.querySelector("#signupConfirmPassword").value
      };

      fetch(registerUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify(payload)
      })
        .then(function (response) {
          return response.text().then(function (text) {
            var body = {};
            if (text) {
              try {
                body = JSON.parse(text);
              } catch (err) {
                body = { message: text };
              }
            }
            return { status: response.status, body: body };
          });
        })
        .then(function (result) {
          isSubmitting = false;
          setLoading(btn, false);

          if (result.status === 201 && result.body && result.body.success) {
            showAlert(form, "success", result.body.message || "User registered successfully.");
            form.reset();
            setTimeout(function () {
              window.location.href = loginUrl || "/UserAuthentication/UserLogin";
            }, 1200);
            return;
          }

          if (result.status === 409) {
            showAlert(form, "error", (result.body && result.body.message) || "Email is already registered.");
            return;
          }

          if (result.status === 400 && result.body) {
            if (result.body.errors) {
              Object.keys(result.body.errors).forEach(function (key) {
                var messages = result.body.errors[key];
                var fieldMap = {
                  FullName: "signupFullName",
                  fullName: "signupFullName",
                  Email: "signupEmail",
                  email: "signupEmail",
                  MobileNumber: "signupMobile",
                  mobileNumber: "signupMobile",
                  Password: "signupPassword",
                  password: "signupPassword",
                  ConfirmPassword: "signupConfirmPassword",
                  confirmPassword: "signupConfirmPassword"
                };
                var input = form.querySelector("#" + (fieldMap[key] || ""));
                if (input && messages && messages.length) {
                  showFeedback(input, messages[0], false);
                }
              });
            }
            showAlert(form, "error", result.body.message || "Validation failed.");
            return;
          }

          if (result.status === 502) {
            showAlert(form, "error", (result.body && result.body.message) || "API Gateway is unavailable.");
            return;
          }

          showAlert(form, "error", "Registration failed. Please verify the API Gateway and Portfolio Service are running.");
        })
        .catch(function () {
          isSubmitting = false;
          setLoading(btn, false);
          showAlert(form, "error", "Unable to complete registration. Check that PERSONALAPP, API Gateway, and Portfolio Service are running.");
        });
    });
  }

  initLoginForm();
  initSignUpForm();
})();
