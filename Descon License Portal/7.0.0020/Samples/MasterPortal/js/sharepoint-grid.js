(function ($) {

	function sharepointGrid(element) {
		this._element = $(element);
		this._target = this._element.data("target") || {};
		this._serviceUrlGet = this._element.attr("data-url-get");
		this._serviceUrlAdd = this._element.attr("data-url-add");
		this._serviceUrlDelete = this._element.attr("data-url-delete");
		this._addEnabled = this._element.data("add-enabled");
		this._deleteEnabled = this._element.data("delete-enabled");
		this._pageSize = this._element.attr("data-pagesize");
		this._addSuccess = false;
		this._deleteSuccess = false;
		var that = this;
		$(element).on("refresh", function (e, page) {
			that.load(page);
		});
	}

	$(document).ready(function () {
		$(".sharepoint-grid").each(function () {
			new sharepointGrid($(this)).render();
		});
	});

	sharepointGrid.prototype.render = function () {
		var $this = this;
		var $element = $this._element;
		var $addFileButton = $element.children(".grid-actions").find("a.add-file");
		var $modalAddFile = $element.children(".modal-add-file");
		var $modalAddFileButton = $modalAddFile.find(".modal-footer .btn-primary");

		$this.load();

		if ($this._addEnabled) {
			$addFileButton.on("click", function () {
				var destinationGroup = $modalAddFile.find(".destination-group");
				var folderPath = $element.attr("data-folderpath");
				if (!folderPath) {
					destinationGroup.hide();
				} else {
					destinationGroup.find("p.destination-folder").html(folderPath + "/");
					destinationGroup.show();
				}
				$modalAddFile.modal("show");
			});

			$modalAddFileButton.on("click", function () {
				$this.addFile();
			});

			$modalAddFile.on('hidden.bs.modal', function () {
				$modalAddFile.find("input[type='file']").val('');
				$modalAddFile.find(".alert-danger.error").remove();
			});
		}
	}

	sharepointGrid.prototype.load = function (page) {
		var $this = this;
		var $element = $this._element;
		var $sharepointData = $element.children(".sharepoint-data");
		var $errorMessage = $element.children(".sharepoint-error");
		var $emptyMessage = $element.children(".sharepoint-empty");
		var $accessDeniedMessage = $element.children(".sharepoint-access-denied");
		var $loadingMessage = $element.children(".sharepoint-loading");
		var $pagination = $element.find(".sharepoint-pagination");
		var serviceUrlGet = $this._serviceUrlGet;
		var regarding = $this._target;
		var defaultPageSize = $this._pageSize;

		$errorMessage.hide();
		$emptyMessage.hide();
		$accessDeniedMessage.hide();
		$sharepointData.hide().empty();
		$loadingMessage.show();
		var $sortExpression = $element.attr("data-sort-expression") || "FileLeafRef ASC";
		var pageNumber = $pagination.data("current-page");
		if (pageNumber == null || pageNumber == '') {
			pageNumber = 1;
		}
		page = page || pageNumber;
		var pageSize = $pagination.data("pagesize");
		if (pageSize == null || pageSize == '') {
			pageSize = defaultPageSize;
		}
		var folderPath = $element.attr("data-folderpath") || "";
		var safeFolderPath = folderPath.replace(/[\t\n\f \/>"'<=]/g, "");
		var pagingInfo = $element.attr("data-paging-info" + page + safeFolderPath);
		$this.getData(serviceUrlGet, regarding, $sortExpression, page, pageSize, pagingInfo, folderPath,
			function (data) {
				// done
				if (typeof data === typeof undefined || data === false || data == null) {
					$emptyMessage.fadeIn();
					return;
				}
				if (typeof data.AccessDenied !== typeof undefined && data.AccessDenied !== false && data.AccessDenied) {
					$accessDeniedMessage.fadeIn();
					return;
				}
				if (data.TotalCount == 0) {
					$emptyMessage.fadeIn();
					return;
				}

				var source = $("#sharepoint-template").html();
				var template = Handlebars.compile(source);
				$sharepointData.html(template(data));
				$sharepointData.find("abbr.timeago").each(function () {
					var date = $(this).attr("title");
					var moment = window.moment;
					if (moment) {
						var dateFormat = dateFormatConverter.convert($element.closest("[data-dateformat]").data("dateformat") || "M/d/yyyy", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
						var timeFormat = dateFormatConverter.convert($element.closest("[data-timeformat]").data("timeformat") || "h:mm tt", dateFormatConverter.dotNet, dateFormatConverter.momentJs);
						var datetimeFormat = dateFormat + ' ' + timeFormat;
						$(this).text(moment(date).format(datetimeFormat));
					}
				});

				$sharepointData.find("abbr.timeago").timeago();
				$sharepointData.fadeIn();

				$this.addFolderClickEventHandlers();

				//if ($this._deleteEnabled && $this._deleteEnabled != "False") {
				//	$this.addDeleteClickEventHandlers();
				//}

				var splitExpression = $sortExpression.split(" ");
				var name = splitExpression[0];
				var dir = splitExpression[1];
				$element.find(".view-grid").find("table").find("th.sort-enabled a").each(function() {
					var $header = $(this).closest("th");
					if ($header.data("sort-name") == name) {
						if (dir == "ASC") {
							$header.data("sort-dir", "ASC").removeClass("sort-desc").addClass("sort-asc");
							$header.attr("aria-sort", "ascending");
							$(this).append(" ").append($("<span></span>").addClass("fa").addClass("fa-arrow-up")).append("<span class='sr-only sort-hint'>. Activate to sort in descending order</span>");
						} else {
							$header.data("sort-dir", "DESC").removeClass("sort-asc").addClass("sort-desc");
							$header.attr("aria-sort", "descending");
							$(this).append(" ").append($("<span></span>").addClass("fa").addClass("fa-arrow-down")).append("<span class='sr-only sort-hint'>. Activate to sort in ascending order</span>");
						}
						$header.addClass("sort");
					}
				});
				$this.addSortEventHandlers();

				$this.initializePagination(data, safeFolderPath);
			},
			function (jqXhr, textStatus, errorThrown) {
				// fail
				$errorMessage.find(".details").append(errorThrown);
				$errorMessage.show();
			},
			function () {
				// always
				$loadingMessage.hide();
			});
	}

	sharepointGrid.prototype.getData = function (url, regarding, sortExpression, page, pageSize, pagingInfo, folderPath, done, fail, always) {
		done = $.isFunction(done) ? done : function () { };
		fail = $.isFunction(fail) ? fail : function () { };
		always = $.isFunction(always) ? always : function () { };
		if (!url || url == '') {
			always.call(this);
			fail.call(this, null, "error", "A required service url was not provided.");
			return;
		}
		if (!regarding) {
			always.call(this);
			fail.call(this, null, "error", "A required regarding EntityReference parameter was not provided.");
			return;
		}
		pageSize = pageSize || -1;
		var data = {};
		data.regarding = regarding;
		data.sortExpression = sortExpression;
		data.page = page;
		data.pageSize = pageSize;
		data.pagingInfo = pagingInfo;
		data.folderPath = folderPath;
		var jsonData = JSON.stringify(data);
		$.ajax({
			type: 'POST',
			dataType: "json",
			contentType: 'application/json',
			url: url,
			data: jsonData,
			global: false
		})
			.done(done)
			.fail(fail)
			.always(always);
	}

	sharepointGrid.prototype.addFolderClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;

		$element.find(".folder-link").on("click", function (e) {
			e.preventDefault();

			$element.attr("data-folderpath", $(this).attr("data-folderpath"));

			$this.load(1);
		});
	}

	sharepointGrid.prototype.addFile = function () {
		var $this = this;
		var $element = $this._element;
		var target = $this._target;
		var url = $this._serviceUrlAdd;
		var $modal = $element.find(".modal-add-file");
		var $button = $modal.find(".modal-footer button.primary");
		var files = null;
		var overwrite = false;

		if (url == null || url == '') {
			var urlError = { Message: "System Error", InnerError: { Message: "The URL to the service for this Add File Request could not be determined." } };
			onFail(urlError, $modal);
			return;
		}

		var $file = $modal.find("input[type='file']");

		var $overwrite = $modal.find("input[type='checkbox']");

		if ($overwrite.length > 0) {
			overwrite = $overwrite.prop('checked');
		}

		$button.attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-pulse' aria-hidden='true'></span> ");

		if (window.FormData !== undefined) {
			var formData = new FormData();
			formData.append("regardingEntityLogicalName", target.LogicalName);
			formData.append("regardingEntityId", target.Id);

			if ($file.length > 0) {
				if (typeof ($file)[0].files !== typeof undefined && ($file)[0].files.length > 0) {
					files = ($file)[0].files;
					for (var i = 0; i < files.length; i++) {
						formData.append("files", files[i]);
					}
				}
			}

			formData.append("overwrite", overwrite);

			if ($element.attr("data-folderpath")) {
				formData.append("folderPath", $element.attr("data-folderpath"));
			}

			$.ajax({
				url: url,
				type: 'POST',
				data: formData,
				mimeType: "multipart/form-data",
				contentType: false,
				cache: false,
				processData: false
			}).done(function () {
				$this._addSuccess = true;
				$element.trigger("refresh");
				$modal.modal("hide");
			}).fail(function (jqXhr) {
				var contentType = jqXhr.getResponseHeader("content-type");
				var error = contentType.indexOf("json") > -1 ? $.parseJSON(jqXhr.responseText) : { Message: jqXhr.status, InnerError: { Message: jqXhr.statusText } };
				onFail(error, $modal);
			}).always(function () {
				$button.removeAttr("disabled", "disabled").find(".fa-pulse").remove();
			});
		}
		else {
			onFail({ Message: "Your browser does not support FormData." }, $modal);
		}
	}

	sharepointGrid.prototype.addDeleteClickEventHandlers = function () {
		var $this = this;
		var $element = $this._element;
		var url = $this._serviceUrlDelete;

		var $modal = $element.children(".modal-delete-file");

		if ($modal.length == 0) {
			return;
		}

		$modal.on('hidden.bs.modal', function () {
			$modal.find(".alert-danger.error").remove();
		});

		$element.find(".delete-link").on("click", function (e) {
			e.preventDefault();
			var $spFile = $(this).closest(".sp-file");
			var id = $spFile.data("id");
			if (!id || id == '') {
				console.log("Failed to launch delete file dialog. Data parameter 'id' is null.");
				return;
			}
			var $button = $modal.find(".modal-footer button.primary");
			$button.unbind("click");
			$button.on("click", function () {
				$(this).attr("disabled", "disabled").prepend("<span class='fa fa-spinner fa-pulse' aria-hidden='true'></span>");
				var data = {};
				data.id = id;
				var jsonData = JSON.stringify(data);
				$.ajax({
					type: "POST",
					contentType: "application/json",
					url: url,
					data: jsonData
				}).done(function () {
					$this._deleteSuccess = true;
					$element.trigger("refresh");
					$modal.modal("hide");
				}).fail(function (jqXhr) {
					var contentType = jqXhr.getResponseHeader("content-type");
					var error = contentType.indexOf("json") > -1 ? $.parseJSON(jqXhr.responseText) : { Message: jqXhr.status, InnerError: { Message: jqXhr.statusText } };
					onFail(error, $modal);
				}).always(function () {
					$button.removeAttr("disabled", "disabled").find(".fa-pulse").remove();
				});
			});
			$modal.modal();
		});
	}

	sharepointGrid.prototype.addSortEventHandlers = function () {
		var $this = this;
		var $element = $this._element;

		$element.find(".view-grid").find("table").find("th.sort-enabled a").on("click", function (e) {
			e.preventDefault();
			var $header = $(this).closest("th");
			var name = $header.data("sort-name");
			var dir = $header.data("sort-dir");
			if (typeof name === typeof undefined || name === false || name === null || name === '') {
				return;
			}
			if (typeof dir === typeof undefined || dir === false) {
				dir = "";
			}
			var sortExpression;
			if (dir == "ASC") {
				sortExpression = name + " DESC";
			} else {
				sortExpression = name + " ASC";
			}
			$element.attr("data-sort-expression", sortExpression);
			$this.load(1);
		});
	}

	sharepointGrid.prototype.initializePagination = function (data, safeFolderPath) {
		// requires ~/js/jquery.bootstrap-pagination.js
		if (typeof data === typeof undefined || data === false || data == null) {
			return;
		}

		if ((typeof data.PageSize === typeof undefined || data.PageSize === false || data.PageSize == null)
			|| (typeof data.PageNumber === typeof undefined || data.PageNumber === false || data.PageNumber == null)
			|| (typeof data.TotalCount === typeof undefined || data.TotalCount === false || data.TotalCount == null)) {
			return;
		}

		var $this = this;
		var $element = $this._element;
		var $pagination = $element.find(".view-pagination");

		// find page count
		var pageCount = parseInt($element.attr("data-page-count" + safeFolderPath)) || 1;

		if (data.PagingInfo) {
			// if there's paging info, there's another page; set pagingInfo for the next page
			$element.attr("data-paging-info" + (data.PageNumber + 1) + safeFolderPath, data.PagingInfo);
			// set the page count for an additional page if necessary
			if (pageCount < (data.PageNumber + 1)) {
				pageCount = (data.PageNumber + 1);
				$element.attr("data-page-count" + safeFolderPath, pageCount);
			}
		}

		if (pageCount <= 1) {
			$pagination.hide();
			return;
		}

		$pagination
			.data("pagesize", data.PageSize)
			.data("pages", pageCount)
			.data("current-page", data.PageNumber)
			.data("count", data.TotalCount)
			.unbind("click")
			.pagination({
				total_pages: $pagination.data("pages"),
				current_page: $pagination.data("current-page"),
				callback: function (event, pg) {
					event.preventDefault();
					var $li = $(event.target).closest("li");
					if ($li.not(".disabled").length > 0 && $li.not(".active").length > 0) {
						$this.load(pg);
					}
				}
			})
			.show();
	}

	function onFail(error, $modal) {
		if (typeof error !== typeof undefined && error !== false && error != null) {
			console.log(error);

			var $body = $modal.find(".modal-body");

			var $error = $modal.find(".alert-danger.error");

			if ($error.length == 0) {
				$error = $("<div></div>").addClass("alert alert-block alert-danger error clearfix");
			} else {
				$error.empty();
			}

			if (typeof error.InnerError !== typeof undefined && typeof error.InnerError.Message !== typeof undefined && error.InnerError.Message !== false && error.InnerError.Message != null) {
				if (typeof error.InnerError.Message === 'number') {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.InnerError.Message + " Error</p>");
				} else {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.InnerError.Message + "</p>");
				}
			} else if (typeof error.Message !== typeof undefined && error.Message !== false && error.Message != null) {
				if (typeof error.Message === 'number') {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.Message + " Error</p>");
				} else {
					$error.append("<p><span class='fa fa-exclamation-triangle' aria-hidden='true'></span> " + error.Message + "</p>");
				}
			}

			$body.prepend($error);
		}
	}
}(jQuery));