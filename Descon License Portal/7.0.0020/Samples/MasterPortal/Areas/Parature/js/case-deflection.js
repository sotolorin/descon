/*
# Parature Case Deflection widget

<input type="text" class="form-control parature-deflection"
  data-url="{{ parature.articles.search_url | escape }}"
  data-target="#articles"
  data-template="#article-results" />

<div id="articles"></div>

{% raw %}
<script id="article-results" type="text/x-handlebars-template">
  <div class="list-group">
	{{# each articles}}
	  <a class="list-group-item" href="{{ url }}">{{ question }}</a>
	{{/each}}
  </div>
</script>
{% endraw %}

*/

(function ($, handlebars) {
	'use strict';

  $(document).on('blur.adx.parature.case-deflection', '.parature-deflection', getResults);

	function getResults() {
		var $this = $(this),
			url = $this.data('url'),
			target = $this.data('target'),
			template = $this.data('template'),
			value = $this.val(),
      compiledTemplate;

    if (!(url && target && template && value)) {
			return;
		}

    $(target).html('<div style="text-align: center;"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span></div>');

			compiledTemplate = handlebars.compile($(template).html());

		$.getJSON(url, { keywords: value }, function (data) {
			if (data == null || data.total_items == 0) {
				$target.empty();
				return;
			}
			$target.html(compiledTemplate(data));
			if (!target) {
				$this.parent().find(".articles").remove();
				$this.parent().append($target).show();
			}
		});
	}

}(jQuery, Handlebars));
