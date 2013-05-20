define(["underscore", "ri18n!app/config/localization/nls/labels"], function (_, labels) {
	"use strict";

	var config = {
		mode: "development",

		version: "1.0.2.0",

		name: "EPIS Web UI",

		author: {
			company: "EPAM"
		},

		_labels: labels,

		proxies: {
			epis: {
				url: "http://epis-json.aws.af.cm/rest/",
				dataType: "json",
				timeout: 10000,
				// one hour
				cacheTimeout: 1000*60*60,

				formats: {
					date: "YYYY-MM-DD HH:mm ZZ"
				}
			}
		},

		constants: {
			vacation_states: {
				ALL: -1,
				SUBMITTED: 79,
				APPROVED: 80,
				REJECTED: 81,
				CLOSED: 82,
				IN_PROGRESS: 112,
				CANCELLED: 113,
				DRAFT: 114,
				ARRIVED: 134
			},

			vacation_types: {
				ILL: 1,
				VAC: 3,
				OVT: 6,
				POV: 7,
				EXV: 8
			},

			project_types: {
				ALL: -1,
				MY_PROJECTS: -100
			},

			employee_types:{
				ALL: -1,
				ME: -3,
				SUBORDINATES: -2
			}
		},

		state: {
			storageKey: "epis_state.json"
		},

		formats: {
			date: "M/D/YY",
			dateWithTime: "M/D/YY h:mm A"
		},

		getString: function(key){
			return this._labels[key];
		}
	};

	if(config.mode === "development"){
		config.proxies.epis.url = "http://192.168.1.124:8080/pmc_json/rest/";
	}

	return config;
});